using Microsoft.AspNetCore.Mvc;

namespace RoleMgtMVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
