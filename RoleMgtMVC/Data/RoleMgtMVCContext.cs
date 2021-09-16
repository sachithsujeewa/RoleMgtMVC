using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RoleMgtMVC.Controllers;
using RoleMgtMVC.Models;

namespace RoleMgtMVC.Data
{
    public class RoleMgtMVCContext : DbContext
    {
        public RoleMgtMVCContext(DbContextOptions<RoleMgtMVCContext> options)
            : base(options)
        {
        }

        public DbSet<UserRole> UserRole { get; set; }

        public DbSet<User> User { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<UserRole>().HasData(
                new UserRole
                {
                    Id = 1,
                    Name = "External",
                    Level = 1
                },
                new UserRole
                {
                    Id = 2,
                    Name = "Internal",
                    Level = 2
                },
                new UserRole
                {
                    Id = 3,
                    Name = "Lead",
                    Level = 3
                },
                new UserRole
                {
                    Id = 4,
                    Name = "Admin",
                    Level = 4
                });

            var hashData = UsersController.GetPasswordHashValue("Qwerty1@");

            builder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    UserRoleId = 4,
                    Email ="sachithsujeewa@gmail.com",
                    Salt = hashData.Salt,
                    PasswordHash = hashData.PasswordHash,
                    CreatedDate = DateTime.Now,
                    FirstName = "Sachith",
                    LastName = "Sujeewa",
                    IsActive = true
                }    
            );
        }

    }
}
