using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace SSOSegurancaMicrosservice.Service
{
    public class RedisSecurityCacheService : ISecurityCacheService
    {
        private string _SecurityCacheKey;
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private int MaxAge;

        public bool IsDefault => false;

        public RedisSecurityCacheService(IConnectionMultiplexer redis, IConfiguration Configuration)
        {
            _redis = redis;
            _database = _redis.GetDatabase();
            _SecurityCacheKey = Configuration["Security:Authentication:AppPrefix"];
            

            var bsuccess = int.TryParse(Configuration["Security:Authentication:Jwt:MaxAge"], out MaxAge);

            if (!bsuccess)
            {
                MaxAge = 60; // padrão de 60 minutos caso não seja definido
            }
        }

        public async Task<List<string>> GetUserRoles(string key)
        {
            var t = Task.Run(async () =>
            {
                var profile = await _database.StringGetAsync($"{_SecurityCacheKey}-{key}");
                if(profile.IsNullOrEmpty)
                {
                    return new List<string>();
                }
                var roles = profile.ToString().Split(',').ToList();
                return roles;
            });
            return await t;
        }

        public async void SetUserRoles(List<string> roles, string key)
        {
            var profile = string.Join(",", roles);
            await _database.StringSetAsync($"{_SecurityCacheKey}-{key}", profile, expiry: TimeSpan.FromMinutes(MaxAge));
        }

        public void RemoveUserRoles(string key)
        {
            _database.StringGetDelete($"{_SecurityCacheKey}-{key}");
        }
    }
}
