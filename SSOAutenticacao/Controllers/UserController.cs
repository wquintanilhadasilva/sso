using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSOAutenticacao.Service;
using SSOSegurancaMicrosservice.Configuration;
using SSOSegurancaMicrosservice.Service;
using System.Threading.Tasks;

namespace SSOAutenticacao.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class UserController: ControllerBase
    {

        private readonly ISecurityCacheService _cache;

        public UserController(ISecurityCacheService cache)
        {
            _cache = cache;
        }

        [HttpGet]
        [Route("details")]
        [Authorize]
        public async Task<IActionResult> Details()
        {
            var token = Request.Cookies[SecurityConfiguration.TOKEN_NAME];

            var t = Task.Run(() => {
                var userProfile = ProfileService.BuildUserProfile(User, _cache, token);
                return Ok(userProfile);
            });
            return await t;
        }
    }
}
