using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SSOSegurancaMicrosservice.Autenticacao;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SSOAutenticacao.Autenticacao
{
    public static class AutenticacaoServiceConfigureExtension
    {

        public static void ConfigureAutenticacaoService(this IServiceCollection services, IConfiguration Configuration, bool UseRedis = false)
        {            
            ConfigureOAuth(services, Configuration);

            if (UseRedis)
            {
                services.ConfigureRedis(Configuration);
            }else
            {
                services.ConfigureDefault(Configuration);
            }
        }

        private static void ConfigureOAuth(IServiceCollection services, IConfiguration Configuration)
        {

            services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = "oidc";
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;

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
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "id");
                options.ClaimActions.MapJsonSubKey(ClaimTypes.Name, "attributes", "displayName");
                options.ClaimActions.MapJsonSubKey(ClaimTypes.Role, "attributes", "Role");

                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                        var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                        response.EnsureSuccessStatusCode();
                        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                        context.RunClaimActions(json.RootElement);
                        var roles = context.Identity.FindFirst(ClaimTypes.Role)?.Value;
                        var subject = context.Identity.FindFirst(ClaimTypes.NameIdentifier).Value;
                        if (roles != null)
                        {
                            foreach (var claim in Regex.Replace(roles, @"[\[\s\]\""]+", "").Split(","))
                            {
                                context.Identity.AddClaim(new Claim(ClaimTypes.Role, claim.Trim()));
                            }
                        }
                    },
                };
            });
        }

    }
}
