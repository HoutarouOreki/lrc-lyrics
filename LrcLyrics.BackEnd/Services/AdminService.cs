using LrcLyrics.BackEnd.Models;
using MongoDB.Driver;

namespace LrcLyrics.BackEnd.Services
{
    public class AdminService
    {
        private readonly IMongoCollection<Key> keys;

        public AdminService()
        {
            var client = new MongoClient(Utilities.ConnectionString);
            var database = client.GetDatabase("lrc-lyrics");
            if (!database.ListCollectionNames().ToList().Contains("keys"))
                database.CreateCollection("keys");
            keys = database.GetCollection<Key>("keys");
        }

        public bool ValidateKey(string key)
        {
            if (keys.Find(k => k.Value == (key ?? "")).CountDocuments() == 1)
                return true;
            return false;
        }
    }
}
