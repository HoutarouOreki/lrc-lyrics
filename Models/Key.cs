using MongoDB.Bson.Serialization.Attributes;

namespace LrcLyrics.Models
{
    public class Key
    {
        [BsonElement("value")]
        public string Value { get; set; }
    }
}