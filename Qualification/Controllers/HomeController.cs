using CommonLogger;
using Microsoft.AspNetCore.Mvc;
using Qualification.Models;
using System.Diagnostics;

namespace Qualification.Controllers
{
    public class HomeController(IAILogger logger) : Controller
    {
        public IActionResult Index()
        {
            logger.LogInformation("Navigated to Home / Index page");
            return View();
        }

        public IActionResult Privacy()
        {
            logger.LogInformation("Navigated to Home / Privacy page");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            logger.LogWarning("Navigated to Home / Error page");
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
