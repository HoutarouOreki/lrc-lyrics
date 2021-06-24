using LrcLyrics.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace LrcLyrics.Services
{
    public class FeedbackService
    {
        private readonly IMongoCollection<Feedback> feedbacks;

        public FeedbackService()
        {
            var client = new MongoClient(Utilities.ConnectionString);
            var database = client.GetDatabase("lrc-lyrics");
            var collectionNames = database.ListCollectionNames().ToList();
            if (!collectionNames.Contains("feedback"))
                database.CreateCollection("feedback");
            feedbacks = database.GetCollection<Feedback>("feedback");
        }

        public Feedback GetFeedback(int id)
        {
            var request = feedbacks.Find(r => r.Id == id).FirstOrDefault();
            return request;
        }

        public void AddFeedback(Feedback feedback)
        {
            var highestId = feedbacks.Find(_ => true).SortByDescending(l => l.Id).Limit(1).FirstOrDefault()?.Id ?? 0;
            feedback.Id = highestId + 1;
            feedbacks.InsertOne(feedback);
        }

        public IReadOnlyList<Feedback> GetOpen() => feedbacks.Find(r => r.State == (int)SubmissionState.Pending).ToList();

        public IReadOnlyList<Feedback> GetClosed() => feedbacks.Find(r => ((int)r.State == (int)SubmissionState.Denied) || ((int)r.State == (int)SubmissionState.Published)).ToList();

        public void CloseFeedback(int id)
        {
            var update = Builders<Feedback>.Update.Set(lr => lr.State, SubmissionState.Published);
            feedbacks.UpdateOne(lr => lr.Id == id, update);
        }
    }
}
