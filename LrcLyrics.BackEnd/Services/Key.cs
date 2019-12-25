using MongoDB.Bson.Serialization.Attributes;

namespace LrcLyrics.BackEnd.Services
{
    public class Key
    {
        [BsonElement("value")]
        public string Value { get; set; }
    }
}