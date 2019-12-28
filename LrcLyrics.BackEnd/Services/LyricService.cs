using LrcLyrics.BackEnd.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LrcLyrics.BackEnd.Services
{
    public class LyricService
    {
        private readonly IMongoCollection<Lyrics> lyrics;
        private readonly IMongoCollection<LyricsSubmission> lyricSubmissions;

        public LyricService()
        {
            var client = new MongoClient(Utilities.ConnectionString);
            var database = client.GetDatabase("lrc-lyrics");
            var collectionNames = database.ListCollectionNames().ToList();
            if (!collectionNames.Contains("lyrics"))
                database.CreateCollection("lyrics");
            if (!collectionNames.Contains("lyric-submissions"))
                database.CreateCollection("lyric-submissions");
            lyrics = database.GetCollection<Lyrics>("lyrics");
            lyricSubmissions = database.GetCollection<LyricsSubmission>("lyric-submissions");
        }

        public Lyrics GetLyrics(int id, bool incrementVisits = false)
        {
            var lyric = lyrics.Find(l => l.Id == id).FirstOrDefault();
            if (incrementVisits && lyric != null)
            {
                var update = Builders<Lyrics>.Update.Inc(l => l.Visits, 1);
                lyrics.UpdateOne(l => l.Id == id, update);
                lyric.Visits++;
            }
            return lyric;
        }

        public void IncrementLyricsDownloads(int id)
        {
            var update = Builders<Lyrics>.Update.Inc(l => l.Downloads, 1);
            lyrics.UpdateOne(l => l.Id == id, update);
        }

        public void AddLyrics(Lyrics addedLyrics)
        {
            var highestId = lyrics.Find(_ => true).SortByDescending(l => l.Id).Limit(1).FirstOrDefault()?.Id ?? 0;
            addedLyrics.Id = highestId + 1;
            lyrics.InsertOne(addedLyrics);
        }

        public void UpdateLyrics(Lyrics nl)
        {
            var update = Builders<Lyrics>.Update
                .Set(l => l.Artist, nl.Artist)
                .Set(l => l.Creators, nl.Creators)
                .Set(l => l.DateUpdated, DateTime.UtcNow)
                .Set(l => l.Description, nl.Description)
                .Set(l => l.Lines, nl.Lines)
                .Set(l => l.MusicUrl, nl.MusicUrl)
                .Set(l => l.Source, nl.Source)
                .Set(l => l.Title, nl.Title)
                .Set(l => l.Url, nl.Url)
                .Set(l => l.Visits, nl.Visits);
            lyrics.UpdateOne(l => l.Id == nl.Id, update);
        }

        public IReadOnlyList<Lyrics> GetPopular(int amount = 20) => lyrics.Find(_ => true).SortByDescending(l => l.Visits).Limit(amount).ToList();

        public IReadOnlyList<Lyrics> GetRecent(int amount = 20) => lyrics.Find(_ => true).SortByDescending(l => l.Id).Limit(amount).ToList();

        public IReadOnlyList<LyricsSubmission> GetSubmissions(bool all = false)
        {
            var submissions = lyricSubmissions.Find(_ => true).SortByDescending(l => l.Id).ToList();
            if (all)
                return submissions;
            return submissions.Where(s => s.State == SubmissionState.Pending).ToList();
        }

        public async Task<IReadOnlyList<Lyrics>> Search(string searchText)
        {
            var arr = await lyrics.FindAsync(_ => true);
            var titlesArtists = new List<Lyrics>();
            var texts = new List<Lyrics>();
            foreach (var lyric in await arr.ToListAsync())
            {
                if (lyric.Title.Contains(searchText, StringComparison.InvariantCultureIgnoreCase) || lyric.Artist.Contains(searchText, StringComparison.InvariantCultureIgnoreCase))
                    titlesArtists.Add(lyric);
                else if (texts.Count < 20 && lyric.Lines.Any(l => l.Text.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)))
                    texts.Add(lyric);
                if (titlesArtists.Count == 20)
                    break;
            }
            return new List<Lyrics>(titlesArtists.Concat(texts));
        }

        public void AddSubmission(LyricsSubmission submission)
        {
            var highestId = lyricSubmissions.Find(_ => true).SortByDescending(l => l.Id).Limit(1).FirstOrDefault()?.Id ?? -1;
            submission.Id = highestId + 1;
            submission.Keys.Add($"{submission.Id}-{Utilities.KeyGenerator(10)}");
            lyricSubmissions.InsertOne(submission);
        }

        public LyricsSubmission GetSubmission(int id)
        {
            var lyric = lyricSubmissions.Find(l => l.Id == id).FirstOrDefault();
            return lyric;
        }

        public void UpdateSubmission(LyricsSubmission lyricsSubmission)
        {
            var filter = Builders<LyricsSubmission>.Filter.Eq(ls => ls.Id, lyricsSubmission.Id);
            lyricSubmissions.ReplaceOne(filter, lyricsSubmission);
        }

        public void PublishSubmission(int id)
        {
            var submission = GetSubmission(id);
            if (submission == null)
                throw new Exception("Submission does not exist");
            var lyricsId = (lyrics.Find(_ => true).SortByDescending(l => l.Id).Limit(1).FirstOrDefault()?.Id ?? 0) + 1;
            submission.AcceptedId = lyricsId;
            submission.Lyrics.Id = lyricsId;
            submission.Lyrics.DateAccepted = DateTime.UtcNow;
            submission.Lyrics.DateUpdated = DateTime.UtcNow;
            submission.State = SubmissionState.Published;
            lyricSubmissions.ReplaceOne(Builders<LyricsSubmission>.Filter.Eq(ls => ls.Id, id), submission);
            lyrics.InsertOne(submission.Lyrics);
        }

        public void EditLyrics(int submissionId)
        {
            var submission = GetSubmission(submissionId);
            if (submission == null || !submission.EditId.HasValue)
                throw new Exception("Submission does not exist or has no edit target");
            var lyrics = GetLyrics(submission.EditId.Value);
            submission.AcceptedId = lyrics.Id;
            submission.Lyrics.DateUpdated = DateTime.UtcNow;
            submission.State = SubmissionState.Published;
            lyricSubmissions.ReplaceOne(Builders<LyricsSubmission>.Filter.Eq(ls => ls.Id, submissionId), submission);
            this.lyrics.ReplaceOne(Builders<Lyrics>.Filter.Eq(l => l.Id, lyrics.Id), submission.Lyrics);
        }
    }
}
