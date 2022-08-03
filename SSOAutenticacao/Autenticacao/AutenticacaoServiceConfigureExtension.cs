using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SSOSegurancaMicrosservice.Autenticacao;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace SSOAutenticacao.Autenticacao
{
    public static class AutenticacaoServiceConfigureExtension
    {

        private const string ATTRIBUTES_KEY = "attributes";
        private const string ROLES_ATTRIBUTES_KEY = "authorities";

        public static void ConfigureAutenticacaoService(this IServiceCollection services, IConfiguration Configuration, bool UseRedis = false)
        {
            string RolesAttributesKey = Configuration["Security:OAuth2:RolesAttribute"] ?? ROLES_ATTRIBUTES_KEY;
            ConfigureOAuth(services, Configuration, RolesAttributesKey);
            if (UseRedis)
            {
                services.ConfigureRedis(Configuration);
            }
            else
            {
                services.ConfigureDefault(Configuration);
            }
        }

        private static void ConfigureOAuth(IServiceCollection services, IConfiguration Configuration, string RolesAttributesKey)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = "oidc";
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

            })
            .ConfigureJwt(Configuration)
            .AddCookie(options => 
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.Name = "Cookie";
            })
            .AddOAuth("oidc", options =>
            {
                options.ClientId = Configuration["Security:OAuth2:ClientId"];
                options.ClientSecret = Configuration["Security:OAuth2:Secret"];
                options.CallbackPath = new PathString(Configuration["Security:OAuth2:UserAuthenticationUri"]);
                options.AuthorizationEndpoint = Configuration["Security:OAuth2:UserAuthorizationUri"];
                options.TokenEndpoint = Configuration["Security:OAuth2:AccessTokenUri"];
                options.UserInformationEndpoint = Configuration["Security:OAuth2:UserInfoUri"];
                options.SaveTokens = true;
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "id");
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                options.ClaimActions.MapJsonKey("sub", "id");
                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = OnCreatingTicket,
                    OnRedirectToAuthorizationEndpoint = OnRedirectToAuthorizationEndpoint
                };
            });

        }

        private static Task OnRedirectToAuthorizationEndpoint(RedirectContext<OAuthOptions> ctx)
        {
            var redirectUri = ctx.RedirectUri;
            if (!redirectUri.Contains("redirect_uri=https")) 
            {
                redirectUri = redirectUri.Replace("redirect_uri=http", "redirect_uri=https");
            }
            ctx.HttpContext.Response.Redirect(redirectUri);
            return Task.FromResult(0);
        }

        private static async Task OnCreatingTicket(OAuthCreatingTicketContext context)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
            var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
            response.EnsureSuccessStatusCode();
            var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            context.RunClaimActions(json.RootElement);
            var attributes = new JsonElement();
            var existeAttributes = json.RootElement.TryGetProperty(ATTRIBUTES_KEY, out attributes);
            if (existeAttributes)
            {
                var authorities = new JsonElement();
                var existeAuthorities = attributes.TryGetProperty(ROLES_ATTRIBUTES_KEY, out authorities);
                if (existeAuthorities)
                {
                    string stringRoles = Regex.Replace(authorities.ToString(), @"[\[\s\]\""]+", "");
                    if (!string.IsNullOrEmpty(stringRoles))
                    {
                        foreach (var claim in stringRoles.Split(","))
                        {
                            context.Identity.AddClaim(new Claim(ClaimTypes.Role, claim.Trim()));
                        }
                    }
                }
            }
        }

    }

}
