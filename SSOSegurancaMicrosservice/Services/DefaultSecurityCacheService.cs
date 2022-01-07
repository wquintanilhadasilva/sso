using Microsoft.Extensions.Configuration;
using SSOSegurancaMicrosservice.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSOSegurancaMicrosservice.Services
{
    public class DefaultSecurityCacheService : ISecurityCacheService
    {
        private string _SecurityCacheKey;
        private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();

        public bool IsDefault => true;

        public DefaultSecurityCacheService(IConfiguration Configuration)
        {
            _SecurityCacheKey = Configuration["Security:Authentication:AppPrefix"];
        }

        public async Task<List<string>> GetUserRoles(string key)
        {
            var t = Task.Run(() =>
            {
                var profile = _cache[$"{_SecurityCacheKey}-{key}"];
                if (string.IsNullOrEmpty(profile))
                {
                    return new List<string>();
                }
                var roles = profile.ToString().Split(',').ToList();
                return roles;
            });
            return await t;
        }

        public void RemoveUserRoles(string key)
        {
            _cache.Remove($"{_SecurityCacheKey}-{key}");
        }

        public void SetUserRoles(List<string> roles, string key)
        {
            var profile = string.Join(",", roles);
            _cache.Add($"{_SecurityCacheKey}-{key}", profile);
        }
    }
}
