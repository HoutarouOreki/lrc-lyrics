using MongoDB.Bson.Serialization.Attributes;
using System;

namespace LrcLyrics.Models
{
    public class LyricsRequest
    {
        [BsonElement("id")]
        public int Id { get; set; }

        [BsonElement("artist")]
        public string Artist { get; set; }

        [BsonElement("comments")]
        public string Comments { get; set; }

        [BsonElement("date")]
        public DateTime Date { get; set; }

        [BsonElement("denyReason")]
        public string DenyReason { get; set; }

        [BsonElement("fulfilledId")]
        public int? FulfilledId { get; set; }

        [BsonElement("lyricsLink")]
        public string LyricsLink { get; set; }

        [BsonElement("musicLink")]
        public string MusicLink { get; set; }

        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("state")]
        public SubmissionState State { get; set; }
    }
}
