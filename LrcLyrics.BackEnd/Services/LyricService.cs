using LrcLyrics.BackEnd.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public Lyrics GetLyrics(int id)
        {
            var lyric = lyrics.Find(l => l.Id == id).FirstOrDefault();
            return lyric;
        }

        public void AddLyrics(Lyrics addedLyrics)
        {
            var highestId = lyrics.Find(_ => true).SortByDescending(l => l.Id).Limit(1).FirstOrDefault()?.Id ?? 0;
            addedLyrics.Id = highestId + 1;
            lyrics.InsertOne(addedLyrics);
        }

        public void UpdateLyrics(Lyrics addedLyrics) => lyrics.ReplaceOne(l => l.Id == addedLyrics.Id, addedLyrics);

        public IReadOnlyList<Lyrics> GetRecent() => lyrics.Find(_ => true).SortByDescending(l => l.Id).Limit(20).ToList();

        public IReadOnlyList<LyricsSubmission> GetSubmissions(bool all = false)
        {
            var submissions = lyricSubmissions.Find(_ => true).SortByDescending(l => l.Id).ToList();
            if (all)
                return submissions;
            return submissions.Where(s => s.State == SubmissionState.Pending).ToList();
        }

        public IReadOnlyList<Lyrics> Search(string searchText)
        {
            var search = Builders<Lyrics>.Filter.Text(searchText);
            return lyrics.Find(search).ToList();
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
