using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SSOAutenticacao.Autenticacao;
using SSOSegurancaMicrosservice.Configuration;
using SSOSegurancaMicrosservice.Service;
using System;

namespace SSOAutenticacao.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class SsoController : ControllerBase
    {

        private readonly String loginSuccess;
        private readonly IConfiguration Configuration;
        private readonly ISecurityCacheService _cache;

        public SsoController(IConfiguration configuration, ISecurityCacheService cache)
        {
            this.loginSuccess = configuration.GetValue<string>("Security:OAuth2:LoginSuccess");
            this.Configuration = configuration;
            this._cache = cache;
        }

        [HttpGet]
        [Route("login")]
        [Authorize]
        public IActionResult Get()
        {
            // Chegar aqui, gera o jwt-token no cookie para a aplicação client
            AuthenticationHandler.Authenticated(Response, User, Configuration, _cache);
            return Redirect(loginSuccess);
        }

        [HttpGet]
        [Route("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            //HttpContext.Response.Cookies.Delete("jwt-token");
            //await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Após logout no SSO, faz logout na aplicação
            if(_cache != null)
            {
                var token = Request.Cookies[SecurityConfiguration.TOKEN_NAME];
                _cache.RemoveUserRoles(token);
            }
            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, "oidc", SecurityConfiguration.TOKEN_NAME);
        }
    }
}
