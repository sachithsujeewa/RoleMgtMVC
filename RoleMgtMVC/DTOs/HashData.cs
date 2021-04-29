using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoleMgtMVC.DTOs
{
    public class HashData
    {
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
    }
}
