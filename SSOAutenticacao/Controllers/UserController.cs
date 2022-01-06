using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSOAutenticacao.Service;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SSOAutenticacao.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class UserController: ControllerBase
    {

        [HttpGet]
        [Route("details")]
        [Authorize]
        public async Task<IActionResult> Details()
        {
            var t = Task.Run(() => {
                var userProfile = ProfileService.BuildUserProfile(User);
                return Ok(userProfile);
            });
            return await t;
        }
    }
}
