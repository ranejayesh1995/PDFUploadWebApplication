using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PDFUploadWebApplication.Models;

namespace PDFUploadWebApplication.Controllers
{
    //[Authorize] // Requires authentication for all actions in this controller
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var token = TempData["JwtToken"] as string;
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth"); // Redirect if no token is found
            }
            return View("Index");

        }

        public IActionResult Privacy()
        {
            var token = TempData["JwtToken"] as string;
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth"); // Redirect if no token is found
            }
            return View();
        }

        [AllowAnonymous] // Allows anyone to access error pages
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
