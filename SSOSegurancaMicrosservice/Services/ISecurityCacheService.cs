using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSOSegurancaMicrosservice.Service
{
    public interface ISecurityCacheService
    {
        Task<List<string>> GetUserRoles(string key);
        void SetUserRoles(List<string> roles, string key);

        void RemoveUserRoles(string key);
    }
}
