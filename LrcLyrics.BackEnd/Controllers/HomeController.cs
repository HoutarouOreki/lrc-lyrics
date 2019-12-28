using LrcLyrics.BackEnd.Models;
using LrcLyrics.BackEnd.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LrcLyrics.BackEnd.Controllers
{
    public class HomeController : Controller
    {
        private readonly LyricService lyricService;

        public HomeController(LyricService _lyricService) => lyricService = _lyricService;

        public IActionResult About() => View();

        public IActionResult Index()
        {
            ViewData["Recent"] = lyricService.GetRecent(5);
            ViewData["Popular"] = lyricService.GetPopular(5);
            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
