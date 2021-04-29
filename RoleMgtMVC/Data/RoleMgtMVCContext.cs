using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RoleMgtMVC.Models;

namespace RoleMgtMVC.Data
{
    public class RoleMgtMVCContext : DbContext
    {
        public RoleMgtMVCContext (DbContextOptions<RoleMgtMVCContext> options)
            : base(options)
        {
        }

        public DbSet<UserRole> UserRole { get; set; }

        public DbSet<User> User { get; set; }
    }
}
