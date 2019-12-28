using LrcLyrics.BackEnd.Models;
using LrcLyrics.BackEnd.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LrcLyrics.BackEnd.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RequestsController : Controller
    {
        private readonly RequestService requestService;

        public RequestsController(RequestService _requestService) => requestService = _requestService;

        public IActionResult Index()
        {
            ViewData["Requests"] = requestService.GetOpen();
            return View();
        }

        [HttpGet("Closed")]
        public IActionResult Closed()
        {
            ViewData["Requests"] = requestService.GetClosed();
            return View();
        }

        [HttpGet("New")]
        public IActionResult New() => View();

        [HttpPost("New")]
        public IActionResult New([FromForm]string artist, [FromForm]string title, [FromForm]string lyricsUrl, [FromForm]string musicUrl, [FromForm]string comments)
        {
            var request = new LyricsRequest
            {
                Artist = artist,
                Comments = comments,
                Date = DateTime.UtcNow,
                LyricsLink = lyricsUrl,
                MusicLink = musicUrl,
                Title = title,
                State = SubmissionState.Pending
            };
            requestService.AddRequest(request);
            return RedirectToAction("Index", "Requests");
        }

        [HttpGet("Fulfill/{id:int}")]
        public IActionResult Fulfill(int id)
        {
            var request = requestService.GetRequest(id);
            return RedirectToAction("FulfillRequest", "Lyrics", new { artist = request.Artist, title = request.Title, musicUrl = request.MusicLink, lyricsUrl = request.LyricsLink, comments = request.Comments, requestId = id });
        }
    }
}