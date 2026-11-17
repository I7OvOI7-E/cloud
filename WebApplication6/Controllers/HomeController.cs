using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApplication6.Models;

namespace WebApplication6.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(string? role)
        {
            return role switch
            {
                "SCHOOL_STAFF" => View("~/Views/SchoolStaff/Index.cshtml"),
                "RECIPIENT_STAFF" => View("~/Views/RecipientStaff/Index.cshtml"),
                "VOLUNTEER" => View("~/Views/Volunteer/Index.cshtml"),
                "ADMIN" => View("~/Views/Admin/Index.cshtml"),
                _ => View()
            };
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View("~/Views/Account/Register.cshtml");
        }

        public IActionResult Login()
        {
            return View("~/Views/Account/Login.cshtml");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
