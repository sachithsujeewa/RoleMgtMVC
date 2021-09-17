using Microsoft.AspNetCore.Mvc;
using RoleMgtMVC.Data;
using RoleMgtMVC.Models;
using System;
using System.Text;
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
            if(login.Username == null)
            {
                ModelState.AddModelError("Username", "Username is required...!");
                return View("Login", login);
            }

            if (login.Password == null)
            {
                ModelState.AddModelError("Password", "Password is required...!");
                return View("Login", login);
            }

            var user  = await usersController.GetUser(login.Username);

            if(user == null)
            {
                ModelState.AddModelError("Username", "Incorrect Username...!");
                return View("Login", login);
            }

            var hashData = UsersController.GetPasswordHashValue(login.Password, user.Salt);

            if (hashData.PasswordHash.Equals(user.PasswordHash))
            {
                user.SessionKey = Guid.NewGuid();
                await usersController.UpdateUser(user.Id, user);

                return RedirectToAction("Index", "Users", new { UserId = user.Id, Session = user.SessionKey });
            }
            else
            {
                ModelState.AddModelError("Password", "Incorrect Password...!");
                return View("Login", login);
            }

            //create new session key
            
        }
    }
}
