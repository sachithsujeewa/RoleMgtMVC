using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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

        public HomeController(RoleMgtMVCContext context, IConfiguration configuration)
        {
            _context = context;
            usersController = new UsersController(_context,configuration);
        }

        public IActionResult Index()
        {
            HttpContext.Session.SetString("SessionKey", "");
            HttpContext.Session.SetString("UserId", "");
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

            var user  = await usersController.GetUserByEmail(login.Username);

            if(user == null)
            {
                ModelState.AddModelError("Username", "Incorrect Username...!");
                return View("Login", login);
            }

            if (user.IsActive == false)
            {
                ModelState.AddModelError("Username", "Username should be activated...!");
                return View("Login", login);
            }

            var hashData = UsersController.GetPasswordHashValue(login.Password, user.Salt);

            if (hashData.PasswordHash.Equals(user.PasswordHash))
            {
                user.SessionKey = Guid.NewGuid();
                await usersController.UpdateUser(user.Id, user);

                HttpContext.Session.SetString("SessionKey", user.SessionKey.ToString());
                HttpContext.Session.SetString("UserId", user.Id.ToString());

                return RedirectToAction("Index", "Users");
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
