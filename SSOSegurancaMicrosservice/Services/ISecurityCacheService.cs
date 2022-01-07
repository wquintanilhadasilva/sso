using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSOSegurancaMicrosservice.Service
{
    public interface ISecurityCacheService
    {
        bool IsDefault { get; }
        Task<List<string>> GetUserRoles(string key);
        void SetUserRoles(List<string> roles, string key);

        void RemoveUserRoles(string key);
    }
}
