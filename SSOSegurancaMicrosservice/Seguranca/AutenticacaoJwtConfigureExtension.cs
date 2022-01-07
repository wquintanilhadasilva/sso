using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SSOSegurancaMicrosservice.Configuration;
using SSOSegurancaMicrosservice.Service;
using StackExchange.Redis;
using System.Text;

namespace SSOSegurancaMicrosservice.Autenticacao
{
    public static class AutenticacaoJwtConfigureExtension
    {

        public static void ConfigureAutenticacaoJwt(this IServiceCollection services, IConfiguration Configuration, bool UseRedis = true)
        {
            var secret = Configuration[SecurityConfiguration.SECRET_KEY_CONFIG];
            var key = Encoding.ASCII.GetBytes(secret);

            services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;

            })
            .ConfigureJwt(Configuration);

            if (UseRedis)
            {
                services.ConfigureRedis(Configuration);
            }
        }

        public static AuthenticationBuilder ConfigureJwt(this AuthenticationBuilder builder, IConfiguration Configuration)
        {
            var secret = Configuration[SecurityConfiguration.SECRET_KEY_CONFIG];
            var key = Encoding.ASCII.GetBytes(secret);

            builder.AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            return builder;
        }

        public static void ConfigureRedis(this IServiceCollection services, IConfiguration Configuration)
        {
            //Configure other services up here
            var multiplexer = ConnectionMultiplexer.Connect(Configuration["RedisConnectionString"]);
            services.AddSingleton<IConnectionMultiplexer>(multiplexer);
            services.AddSingleton<ISecurityCacheService, SecurityCacheService>();
        }

    }
}
