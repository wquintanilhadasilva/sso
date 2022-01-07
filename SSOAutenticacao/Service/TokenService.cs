using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SSOAutenticacao.Models;
using SSOSegurancaMicrosservice.Configuration;
using SSOSegurancaMicrosservice.Service;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SSOAutenticacao.Service
{
    public class TokenService
    {
        private string secret;
        private readonly ISecurityCacheService _cache;

        public TokenService(IConfiguration configuration, ISecurityCacheService cache)
        {
            this.secret = configuration.GetValue<string>(SecurityConfiguration.SECRET_KEY_CONFIG);
            this._cache = cache;
        }

        public string GenerateToken(UserProfile user, int MaxAge)
        {
            
            string token;
            // Se tiver a referencia do cache, grava as roles no cache e não no token
            if (_cache != null && !_cache.IsDefault)
            {
                List<string> roles = new List<string>(user.Role);

                user.Role.Clear();
                token = buildToken(user, MaxAge);
                _cache.SetUserRoles(roles, token);
            }else
            {
                token = buildToken(user, MaxAge);
            }

            return token;

        }

        private string buildToken(UserProfile user, int MaxAge)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);

            var claimsList = new List<Claim>();
            claimsList.Add(new Claim(ClaimTypes.Name, user.Name));
            claimsList.Add(new Claim(ClaimTypes.AuthenticationMethod, user.Provider));

            foreach (var role in user.Role)
            {
                if (!string.IsNullOrEmpty(role))
                {
                    claimsList.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claimsList),
                Expires = DateTime.UtcNow.AddHours(MaxAge),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

}
