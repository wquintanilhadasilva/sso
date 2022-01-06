
using System.Collections.Generic;

namespace SSOAutenticacao.Models
{
    public class UserProfile
    {
        public string Name { get; set; }
        public string Provider { get; set; }
        public List<string> Role { get; set; }

    }
}
