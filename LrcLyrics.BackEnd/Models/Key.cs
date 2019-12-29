using MongoDB.Bson.Serialization.Attributes;

namespace LrcLyrics.BackEnd.Models
{
    public class Key
    {
        [BsonElement("value")]
        public string Value { get; set; }
    }
}