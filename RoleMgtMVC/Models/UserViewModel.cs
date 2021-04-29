using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace RoleMgtMVC.Models
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string UserRoleId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public IList<SelectListItem> UserRoles { get; set; }
    }
}
