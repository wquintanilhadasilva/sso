using SSOAutenticacao.Models;
using System.Collections.Generic;
using System.Security.Claims;

namespace SSOAutenticacao.Service
{
    internal class ProfileService
    {

        public static List<string> GetRoles(IEnumerable<Claim> claims)
        {
            var result = new List<string>();
            if(claims != null)
            {
                foreach(var claim in claims)
                {
                    result.Add(claim.Value);
                }
            }
            return result;
        }

        public static UserProfile BuildUserProfile(ClaimsPrincipal Principal)
        {

            // Obtem o token do SSO
            //var token = await HttpContext.GetTokenAsync("access_token");
            if(Principal == null)
            {
                return new UserProfile();
            }

            var user = Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userProfile = new UserProfile
            {
                Name = user,
                Provider = Principal.Identity?.AuthenticationType,
                Role = GetRoles(Principal.FindAll(ClaimTypes.Role))
            };

            return userProfile;
        }
    }
}
