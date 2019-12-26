using LrcLyrics.BackEnd.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LrcLyrics.BackEnd.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult About() => View();

        public IActionResult Index() => View();

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
