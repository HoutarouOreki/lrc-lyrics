﻿using LrcLyrics.Models;
using LrcLyrics.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LrcLyrics.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LyricsController : Controller
    {
        private readonly LyricService lyricService;
        private readonly AdminService adminService;
        private readonly RequestService requestService;
        private readonly Regex lyricRegex = new(@"\[(\d+):(\d+)\.(\d+)\](.*)");

        public LyricsController(LyricService _lyricService, AdminService _adminService, RequestService _requestService)
        {
            lyricService = _lyricService;
            adminService = _adminService;
            requestService = _requestService;
        }

        [HttpGet("RecentlyAdded")]
        public IActionResult RecentlyAdded()
        {
            ViewData["SearchText"] = "Recently added";
            ViewData["SearchResults"] = lyricService.GetRecent();
            return View("Search");
        }

        [HttpGet("Search")]
        public async Task<IActionResult> Search()
        {
            ViewData["SearchText"] = Request.Query["SearchText"];
            ViewData["SearchResults"] = await lyricService.Search(Request.Query["SearchText"]);
            return View();
        }

        [HttpGet("{id}")]
        public IActionResult ViewLyric(int id)
        {
            var lyric = lyricService.GetLyrics(id, true);
            if (lyric == null)
                return View("Error");

            ViewData["Lyric"] = lyric;
            return View("Lyric");
        }

        [HttpGet("{id:int}/{artist:length(0, 100):regex(.*)}/{title:length(0, 100):regex(.*)}")]
        public IActionResult ViewLyric(int id, string _) => ViewLyric(id);

        [HttpGet("{id:int}/{artist:length(0, 100):regex(.*)}/{title:length(0, 100):regex(.*)}/Download")]
        public FileResult DownloadLrc([FromRoute] int id)
        {
            var lyric = lyricService.GetLyrics(id);
            if (lyric != null)
                lyricService.IncrementLyricsDownloads(id);
            return File(Encoding.UTF8.GetBytes(lyric.GetLrcString()), "text/lrc", $"{lyric.Artist} - {lyric.Title}.lrc");
        }

        [HttpGet("{id:int}/{artist:length(0, 100):regex(.*)}/{title:length(0, 100):regex(.*)}/Raw")]
        public IActionResult ViewRaw([FromRoute] int id)
        {
            var lyric = lyricService.GetLyrics(id);
            return Content(lyric.GetLrcString());
        }

        [HttpGet("Submit")]
        public IActionResult Submit() => View("Submit");

        [HttpGet("FulfillRequest")]
        public IActionResult FulfillRequest([FromQuery] string artist, [FromQuery] string title, [FromQuery] string lyricsUrl, [FromQuery] string musicUrl, [FromQuery] string comments, [FromQuery] int requestId)
        {
            ViewData["Submission"] = new LyricsSubmission
            {
                RequestToClose = requestId,
                Lyrics = new Lyrics
                {
                    Artist = artist,
                    Title = title,
                    Source = new LyricsSource
                    {
                        Url = lyricsUrl
                    },
                    MusicUrl = musicUrl,
                    Description = comments
                }
            };
            return Submit();
        }

        [HttpPost("Add")]
        public IActionResult Add([FromForm] string artist, [FromForm] string title, [FromForm] string creators, [FromForm] string musicUrl, [FromForm] string description, [FromForm] string lines, [FromForm] string sourceName, [FromForm] string sourceLink, [FromForm] string sourceDescription, [FromForm] int? requestId, [FromForm] int? editId)
        {
            Lyrics lyrics;
            if (editId.HasValue)
            {
                lyrics = lyricService.GetLyrics(editId.Value);
            }
            else
            {
                lyrics = new Lyrics
                {
                    Artist = artist,
                    Creators = creators,
                    Description = description,
                    MusicUrl = musicUrl,
                    Title = title,
                    Source = new LyricsSource
                    {
                        Description = sourceDescription,
                        Name = sourceName,
                        Type = SourceType.BasedOn,
                        Url = sourceLink
                    }
                };
            }

            lyrics.Lines = ParseLyrics(lines);

            var lyricsSubmission = new LyricsSubmission
            {
                DateSubmitted = DateTime.UtcNow,
                Lyrics = lyrics,
                State = SubmissionState.Pending,
                RawText = lines,
                RequestToClose = requestId,
                EditId = editId
            };

            lyricService.AddSubmission(lyricsSubmission);

            return RedirectToAction("ViewSubmission", "Lyrics", new { id = lyricsSubmission.Id, key = lyricsSubmission.Keys[0] });
        }

        private List<LyricLine> ParseLyrics(string _lines)
        {
            if (string.IsNullOrWhiteSpace(_lines))
                return new List<LyricLine>();
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
                else if (string.IsNullOrWhiteSpace(line) && list.Count != 0)
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
        public IActionResult UpdateSubmission([FromForm] int id, [FromForm] string artist, [FromForm] string title, [FromForm] string creators, [FromForm] string musicUrl, [FromForm] string description, [FromForm] string lines, [FromForm] string key, [FromForm] string sourceName, [FromForm] string sourceLink, [FromForm] string sourceDescription, [FromForm] int? requestId, [FromForm] int? editId)
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

            lyrics.Source = new LyricsSource
            {
                Description = sourceDescription,
                Name = sourceName,
                Type = SourceType.BasedOn,
                Url = sourceLink
            };

            submission.Lyrics = lyrics;
            submission.State = SubmissionState.Pending;
            submission.RawText = lines;
            submission.DateUpdated = DateTime.UtcNow;
            submission.RequestToClose = requestId;
            submission.EditId = editId;

            lyricService.UpdateSubmission(submission);

            return RedirectToAction("ViewSubmission", "Lyrics", new { id });
        }

        [HttpPost("Submissions/Publish")]
        public IActionResult PublishSubmission([FromForm] int id, [FromForm] string key)
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
            if (submission.RequestToClose.HasValue)
                requestService.FulfillRequest(submission.RequestToClose.Value, lyrics.Id);
            return RedirectToAction("ViewLyric", "Lyrics", new { id = lyrics.Id });
        }

        [HttpPost("Submissions/Deny")]
        public IActionResult DenySubmission([FromForm] int id, [FromForm] string key, [FromForm] string denyReason)
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
        public IActionResult EditLyrics([FromQuery] int id)
        {
            var lyric = lyricService.GetLyrics(id);

            ViewData["Submission"] = new LyricsSubmission
            {
                EditBase = string.Join("\r\n", lyric.Lines.Select(l => l.ToString())),
                EditId = lyric.Id,
                DateSubmitted = DateTime.UtcNow,
                Lyrics = lyric,
                State = SubmissionState.Pending,
                RawText = string.Join("\r\n", lyric.Lines.Select(l => l.ToString()))
            };

            return Submit();
        }
    }
}
