﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SSOAutenticacao.Autenticacao;
using SSOSegurancaMicrosservice.Configuration;
using SSOSegurancaMicrosservice.Service;
using System;
using System.Threading.Tasks;

namespace SSOAutenticacao.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class SsoController : ControllerBase
    {

        private readonly String loginSuccess;
        private readonly String ssoLogout;
        private readonly IConfiguration Configuration;
        private readonly ISecurityCacheService _cache;

        public SsoController(IConfiguration configuration, ISecurityCacheService cache)
        {
            this.loginSuccess = configuration.GetValue<string>("Security:OAuth2:LoginSuccess");
            this.ssoLogout = configuration.GetValue<string>("Security:OAuth2:LogoutUri");
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
        public async Task<IActionResult> Logout()
        {
            //HttpContext.Response.Cookies.Delete("jwt-token");
            //await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Após logout no SSO, faz logout na aplicação

            if (User.Identity.IsAuthenticated)
            {
                if (_cache != null && !_cache.IsDefault)
                {
                    var token = Request.Cookies[SecurityConfiguration.TOKEN_NAME];
                    _cache.RemoveUserRoles(token);
                }
                Response.Cookies.Delete(SecurityConfiguration.TOKEN_NAME);
                foreach (var cookie in Request.Cookies.Keys)
                {
                    Response.Cookies.Delete(cookie);
                }

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return Redirect(ssoLogout);
            }
            else
            {
                return Unauthorized();
            }
            
        }
    }
}
