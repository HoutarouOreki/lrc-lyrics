using LrcLyrics.BackEnd.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace LrcLyrics.BackEnd.Services
{
    public class RequestService
    {
        private readonly IMongoCollection<LyricsRequest> requests;

        public RequestService()
        {
            var client = new MongoClient(Utilities.ConnectionString);
            var database = client.GetDatabase("lrc-lyrics");
            var collectionNames = database.ListCollectionNames().ToList();
            if (!collectionNames.Contains("requests"))
                database.CreateCollection("requests");
            requests = database.GetCollection<LyricsRequest>("requests");
        }

        public LyricsRequest GetRequest(int id)
        {
            var request = requests.Find(r => r.Id == id).FirstOrDefault();
            return request;
        }

        public void AddRequest(LyricsRequest request)
        {
            var highestId = requests.Find(_ => true).SortByDescending(l => l.Id).Limit(1).FirstOrDefault()?.Id ?? 0;
            request.Id = highestId + 1;
            requests.InsertOne(request);
        }

        public IReadOnlyList<LyricsRequest> GetOpen() => requests.Find(r => r.State == (int)SubmissionState.Pending).SortBy(r => r.Date).ToList();

        public IReadOnlyList<LyricsRequest> GetClosed() => requests.Find(r => ((int)r.State == (int)SubmissionState.Denied) || ((int)r.State == (int)SubmissionState.Published)).SortBy(r => r.Date).ToList();

        public IReadOnlyList<LyricsRequest> Search(string artist, string title)
        {
            var search = Builders<LyricsRequest>.Filter.Text(artist + " " + title);
            return requests.Find(search).ToList();
        }

        public void FulfillRequest(int id, int lyricsId)
        {
            var update = Builders<LyricsRequest>.Update.Set(lr => lr.State, SubmissionState.Published).Set(lr => lr.FulfilledId, lyricsId);
            requests.UpdateOne(lr => lr.Id == id, update);
        }

        public void DenyRequest(int id, string denyReason)
        {
            var update = Builders<LyricsRequest>.Update.Set(lr => lr.State, SubmissionState.Denied).Set(lr => lr.DenyReason, denyReason);
            requests.UpdateOne(lr => lr.Id == id, update);
        }
    }
}
