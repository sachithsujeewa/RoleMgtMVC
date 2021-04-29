using Microsoft.AspNetCore.Mvc;
using RoleMgtMVC.Models;

namespace RoleMgtMVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var login = new Login();
            return View("Login",login);
        }

        public IActionResult Login(Login login)
        {
            return View();
        }
    }
}
