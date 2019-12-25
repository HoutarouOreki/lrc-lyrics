using LrcLyrics.BackEnd.Models;
using LrcLyrics.BackEnd.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LrcLyrics.BackEnd.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LyricsController : Controller
    {
        private readonly LyricService lyricService;
        private readonly AdminService adminService;
        private readonly Regex lyricRegex = new Regex(@"\[(\d+):(\d+)\.(\d+)\](.*)");

        public LyricsController(LyricService _lyricService, AdminService _adminService)
        {
            lyricService = _lyricService;
            adminService = _adminService;
        }

        [HttpGet("RecentlyAdded")]
        public IActionResult RecentlyAdded()
        {
            ViewData["SearchText"] = "Recently added";
            ViewData["SearchResults"] = lyricService.GetRecent();
            return View("Search");
        }

        [HttpGet("Search")]
        public IActionResult Search()
        {
            ViewData["SearchText"] = Request.Query["SearchText"];
            ViewData["SearchResults"] = lyricService.Search(Request.Query["SearchText"]);
            return View();
        }

        [HttpGet("{id}")]
        public IActionResult ViewLyric(int id)
        {
            var lyric = lyricService.GetLyrics(id);
            if (lyric == null)
                return View("Error");

            ViewData["Lyric"] = lyric;
            return View("Lyric");
        }

        [HttpGet("{id:int}/{artist:length(0, 100):regex(.*)}/{title:length(0, 100):regex(.*)}")]
        public IActionResult ViewLyric(int id, string _) => ViewLyric(id);

        [HttpGet("{id:int}/{artist:length(0, 100):regex(.*)}/{title:length(0, 100):regex(.*)}/Download")]
        public FileResult DownloadLrc([FromRoute]int id)
        {
            var lyric = lyricService.GetLyrics(id);
            return File(Encoding.UTF8.GetBytes(lyric.GetLrcString()), "text/lrc", $"{lyric.Artist} - {lyric.Title}.lrc");
        }

        [HttpGet("{id:int}/{artist:length(0, 100):regex(.*)}/{title:length(0, 100):regex(.*)}/Raw")]
        public IActionResult ViewRaw([FromRoute]int id)
        {
            var lyric = lyricService.GetLyrics(id);
            return Content(lyric.GetLrcString());
        }

        [HttpGet("Submit")]
        public IActionResult Submit() => View();

        [HttpPost("Add")]
        public IActionResult Add([FromForm]string artist, [FromForm]string title, [FromForm]string creators, [FromForm]string musicUrl, [FromForm]string description, [FromForm]string lines)
        {
            var lyrics = new Lyrics
            {
                Artist = artist,
                Creators = creators,
                Description = description,
                MusicUrl = musicUrl,
                Title = title
            };

            lyrics.Lines = ParseLyrics(lines);

            var lyricsSubmission = new LyricsSubmission
            {
                DateSubmitted = DateTime.UtcNow,
                Lyrics = lyrics,
                State = SubmissionState.Pending,
                RawText = lines
            };
            lyricService.AddSubmission(lyricsSubmission);

            return RedirectToAction("ViewSubmission", "Lyrics", new { id = lyricsSubmission.Id, key = lyricsSubmission.Keys[0] });
        }

        private List<LyricLine> ParseLyrics(string _lines)
        {
            var lines = _lines.Replace("\r\n", "\n").Split("\n");
            var list = new List<LyricLine>(lines.Length);
            LyricLine lastEmpty = null;
            foreach (var line in lines)
            {
                var lyricLineMatch = lyricRegex.Match(line);
                if (lyricLineMatch.Success)
                {
                    var minutes = int.Parse(lyricLineMatch.Groups[1].Value);
                    var seconds = int.Parse(lyricLineMatch.Groups[2].Value);
                    var hSeconds = int.Parse(lyricLineMatch.Groups[3].Value);
                    var time = hSeconds + (seconds * 100) + (minutes * 100 * 60);
                    if (lastEmpty != null)
                    {
                        lastEmpty.Time = time;
                        list.Add(lastEmpty);
                        lastEmpty = null;
                    }
                    var text = lyricLineMatch.Groups[4].Value;
                    list.Add(new LyricLine { Text = text, Time = time });
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    lastEmpty = new LyricLine { Text = "" };
                }
            }
            return list;
        }

        [HttpGet("Submissions/{id:int}/{key?}")]
        public IActionResult ViewSubmission(int id, string key)
        {
            var lyric = lyricService.GetSubmission(id);
            if (lyric == null)
                return View("Error");

            ViewData["Lyric"] = lyric;
            ViewData["Key"] = key;
            return View("Submission");
        }

        [HttpGet("Submissions")]
        public IActionResult Submissions()
        {
            ViewData["SearchText"] = "Submissions";
            ViewData["SearchResults"] = lyricService.GetSubmissions();
            return View("Search");
        }

        [HttpGet("Submissions/All")]
        public IActionResult AllSubmissions()
        {
            ViewData["SearchText"] = "Submissions";
            ViewData["SearchResults"] = lyricService.GetSubmissions(true);
            return View("Search");
        }

        [HttpPost("Submissions/Update")]
        public IActionResult UpdateSubmission([FromForm]int id, [FromForm]string artist, [FromForm]string title, [FromForm]string creators, [FromForm]string musicUrl, [FromForm]string description, [FromForm]string lines, [FromForm]string key)
        {
            var submission = lyricService.GetSubmission(id);
            if (!submission.Keys.Contains(key))
            {
                return new BadRequestObjectResult("Key validation failed");
            }
            var lyrics = submission.Lyrics;
            lyrics.Artist = artist;
            lyrics.Creators = creators;
            lyrics.Description = description;
            lyrics.MusicUrl = musicUrl;
            lyrics.Title = title;
            lyrics.Lines = ParseLyrics(lines);

            submission.Lyrics = lyrics;
            submission.State = SubmissionState.Pending;
            submission.RawText = lines;
            submission.DateUpdated = DateTime.UtcNow;

            lyricService.UpdateSubmission(submission);

            return RedirectToAction("ViewSubmission", "Lyrics", new { id });
        }

        [HttpPost("Submissions/Publish")]
        public IActionResult PublishSubmission([FromForm]int id, [FromForm]string key)
        {
            var submission = lyricService.GetSubmission(id);
            if (!adminService.ValidateKey(key))
            {
                return new BadRequestObjectResult("Key validation failed");
            }
            var lyrics = submission.Lyrics;
            if (!submission.EditId.HasValue)
            {
                lyrics.DateAccepted = DateTime.UtcNow;
                lyricService.AddLyrics(lyrics);
            }
            else
            {
                lyrics.DateUpdated = DateTime.UtcNow;
                lyricService.UpdateLyrics(lyrics);
            }
            submission.AcceptedId = lyrics.Id;
            submission.State = SubmissionState.Published;
            lyricService.UpdateSubmission(submission);
            return RedirectToAction("ViewLyric", "Lyrics", new { id = lyrics.Id });
        }

        [HttpPost("Submissions/Deny")]
        public IActionResult DenySubmission([FromForm]int id, [FromForm]string key, [FromForm]string denyReason)
        {
            var submission = lyricService.GetSubmission(id);
            if (!adminService.ValidateKey(key))
            {
                return new BadRequestObjectResult("Key validation failed");
            }
            submission.State = SubmissionState.Denied;
            submission.DeniedReason = denyReason;
            lyricService.UpdateSubmission(submission);
            return RedirectToAction("ViewSubmission", "Lyrics", new { id = submission.Id });
        }

        [HttpGet("EditLyrics")]
        public IActionResult EditLyrics([FromQuery]int id)
        {
            var lyric = lyricService.GetLyrics(id);

            var lyricsSubmission = new LyricsSubmission
            {
                EditBase = string.Join("\r\n", lyric.Lines.Select(l => l.ToString())),
                EditId = lyric.Id,
                DateSubmitted = DateTime.UtcNow,
                Lyrics = lyric,
                State = SubmissionState.Pending,
                RawText = string.Join("\r\n", lyric.Lines.Select(l => l.ToString()))
            };
            lyricService.AddSubmission(lyricsSubmission);

            return RedirectToAction("ViewSubmission", "Lyrics", new { id = lyricsSubmission.Id, key = lyricsSubmission.Keys[0] });
        }
    }
}
