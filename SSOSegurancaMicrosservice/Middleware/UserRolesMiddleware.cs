using Microsoft.AspNetCore.Http;
using SSOSegurancaMicrosservice.Configuration;
using SSOSegurancaMicrosservice.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SSOSegurancaMicrosservice.Middleware
{
    public class UserRolesMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ISecurityCacheService _cache;

        public UserRolesMiddleware(RequestDelegate next, ISecurityCacheService cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            // Se informar o cache, recupera dele as claims do usuário e adiciona ao perfil
            if(_cache != null && !_cache.IsDefault)
            {
                if (httpContext.User != null)
                {
                    var token = httpContext.Request.Cookies[SecurityConfiguration.TOKEN_NAME];
                    var roles = await _cache.GetUserRoles(token);
                    var claims = new List<Claim>();
                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    var appIdentity = new ClaimsIdentity(claims);
                    httpContext.User.AddIdentity(appIdentity);
                }
            } 

            await _next(httpContext);
        }
    }
}
