using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace SSOSegurancaMicrosservice.Service
{
    public class SecurityCacheService : ISecurityCacheService
    {
        private string _SecurityCacheKey;
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public SecurityCacheService(IConnectionMultiplexer redis, IConfiguration Configuration)
        {
            _redis = redis;
            _database = _redis.GetDatabase();
            _SecurityCacheKey = Configuration["Security:OAuth2:ClientId"];
        }

        public async Task<List<string>> GetUserRoles(string key)
        {
            var t = Task.Run(async () =>
            {
                var profile = await _database.StringGetAsync($"{_SecurityCacheKey}-{key}");
                var roles = profile.ToString().Split(',').ToList();
                return roles;
            });
            return await t;
        }

        public async void SetUserRoles(List<string> roles, string key)
        {
            var profile = string.Join(",", roles);
            await _database.StringSetAsync($"{_SecurityCacheKey}-{key}", profile);
        }
    }
}
