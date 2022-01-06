using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SSOAutenticacao.Service;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace SSOAutenticacao.Autenticacao
{
    internal class AuthenticationHandler
    {
        public static void Authenticated(HttpResponse Response, ClaimsPrincipal User, IConfiguration configuration)
        {
            var userProfile = ProfileService.BuildUserProfile(User);

            // Gera o JWT Token para ser usado nas requisições às API's
            var tokenService = new TokenService(configuration);

            var jwtToken = tokenService.GenerateToken(userProfile);

            Response.Cookies.Append("jwt-token", jwtToken, new CookieOptions()
            {
                Path = "/",
                Secure = false,
                HttpOnly = true,
                MaxAge = TimeSpan.FromMinutes(60), //Obter a duração do token gerado pelo SSO
                //SameSite = SameSiteMode.Lax // CSRF-TOKEN
            });

            Response.Headers.Append("Authorization", $"Bearer {jwtToken}");
        }
    }
}
