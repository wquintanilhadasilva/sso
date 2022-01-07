using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SSOAutenticacao.Service;
using SSOSegurancaMicrosservice.Configuration;
using SSOSegurancaMicrosservice.Service;
using System;
using System.Security.Claims;

namespace SSOAutenticacao.Autenticacao
{
    internal class AuthenticationHandler
    {
        public static void Authenticated(HttpResponse Response, 
            ClaimsPrincipal User, IConfiguration configuration, 
            ISecurityCacheService cache)
        {

            // Gera o JWT Token para ser usado nas requisições às API's
            var tokenService = new TokenService(configuration, cache);

            int MaxAge;
            
            var bsuccess = int.TryParse(configuration["Security:Authentication:Jwt:MaxAge"], out MaxAge);

            if (!bsuccess)
            {
                MaxAge = 60; // padrão de 60 minutos caso não seja definido
            }

            var userProfile = ProfileService.BuildUserProfile(User, cache);

            var jwtToken = tokenService.GenerateToken(userProfile, MaxAge);

            Response.Cookies.Append(SecurityConfiguration.TOKEN_NAME, jwtToken, new CookieOptions()
            {
                Path = "/",
                Secure = false,
                HttpOnly = true,
                MaxAge = TimeSpan.FromMinutes(MaxAge),
                //SameSite = SameSiteMode.Lax // CSRF-TOKEN
            });

            Response.Headers.Append("Authorization", $"Bearer {jwtToken}");
        }
    }
}
