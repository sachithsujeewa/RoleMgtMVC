using Microsoft.AspNetCore.Mvc;
using RoleMgtMVC.Data;
using RoleMgtMVC.Models;
using System.Threading.Tasks;

namespace RoleMgtMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly RoleMgtMVCContext _context;
        private readonly UsersController usersController;

        public HomeController(RoleMgtMVCContext context)
        {
            _context = context;
            usersController = new UsersController(_context);
        }

        public IActionResult Index()
        {
            var login = new Login();
            return View("Login",login);
        }

        public async Task<IActionResult> Login(Login login)
        {
            var user  = await usersController.GetUser(login.Username);

            if(user == null)
            {
                ModelState.AddModelError("Username", "Incorrect User Name...!");
                return View("Login", login);
            }
            return RedirectToAction("Index","Users");

            return await new UsersController(_context).Index();
        }
    }
}
