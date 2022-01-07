using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SSOSegurancaMicrosservice.Configuration;
using SSOSegurancaMicrosservice.Middleware;
using SSOSegurancaMicrosservice.Service;
using StackExchange.Redis;
using System;
using System.Text;

namespace SSOSegurancaMicrosservice.Autenticacao
{
    public static class AutenticacaoJwtConfigureExtension
    {

        public static void ConfigureAutenticacaoJwt(this IServiceCollection services, IConfiguration Configuration, bool UseRedis = false)
        {
            var secret = Configuration[SecurityConfiguration.SECRET_KEY_CONFIG];
            if(secret == null)
            {
                throw new SecurityTokenInvalidSigningKeyException($"Chave do Token não informada em [{SecurityConfiguration.SECRET_KEY_CONFIG}].");
            }

            var key = Encoding.ASCII.GetBytes(secret);

            services.AddAuthentication(options =>
            {
                //options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                //options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

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
            if (secret == null)
            {
                throw new SecurityTokenInvalidSigningKeyException($"Chave do Token não informada em [{SecurityConfiguration.SECRET_KEY_CONFIG}].");
            }
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
            var stringConexao = Configuration["RedisConnectionString"];
            if(stringConexao == null)
            {
                throw new ArgumentNullException($"String de conexão com o Redis não informada em [RedisConnectionString].");
            }
            var multiplexer = ConnectionMultiplexer.Connect(stringConexao);
            services.AddSingleton<IConnectionMultiplexer>(multiplexer);
            services.AddSingleton<ISecurityCacheService, SecurityCacheService>();
        }

        /// <summary>
        /// Configura o middlware para obter as permissões do cache no REDIS
        /// </summary>
        /// <param name="app">Referência de IApplicationBuilder</param>
        /// <param name="UseRedis">Define se utiliza o Redis como cache de Roles. Valor padrão é FALSE</param>
        public static void ConfigureSecurityApp(this IApplicationBuilder app)
        {
            app.UseMiddleware<JWTInHeaderMiddleware>();
          
        }

        public static void ConfigureSecurityCacheApp(this IApplicationBuilder app)
        {
            app.UseMiddleware<UserRolesMiddleware>();
        }

    }
}
