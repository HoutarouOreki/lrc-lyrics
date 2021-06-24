using LrcLyrics.Models;
using MongoDB.Driver;
using System.Linq;

namespace LrcLyrics.Services
{
    public class StatisticsService
    {
        private readonly IMongoCollection<SiteStatistics> statistics;

        public StatisticsService()
        {
            var client = new MongoClient(Utilities.ConnectionString);
            var database = client.GetDatabase("lrc-lyrics");
            if (!database.ListCollectionNames().ToList().Contains("statistics"))
                database.CreateCollection("statistics");
            statistics = database.GetCollection<SiteStatistics>("statistics");
            if (statistics.CountDocuments(_ => true) == 0)
                statistics.InsertOne(new SiteStatistics());
        }

        public void IncrementVisit()
        {
            var update = Builders<SiteStatistics>.Update.Inc(s => s.Visits, 1);
            statistics.UpdateOne(_ => true, update);
        }
    }
}
