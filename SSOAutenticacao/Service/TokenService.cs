using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SSOAutenticacao.Models;
using SSOSegurancaMicrosservice.Configuration;
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

        public TokenService(IConfiguration configuration)
        {
            this.secret = configuration.GetValue<string>(SecurityConfiguration.SECRET_KEY_CONFIG);
        }

        public string GenerateToken(UserProfile user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);

            var claimsList = new List<Claim>();
            claimsList.Add(new Claim(ClaimTypes.Name, user.Name));
            claimsList.Add(new Claim(ClaimTypes.AuthenticationMethod, user.Provider));
            foreach (var role in user.Role)
            {
                if(!string.IsNullOrEmpty(role))
                {
                    claimsList.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claimsList),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

}
