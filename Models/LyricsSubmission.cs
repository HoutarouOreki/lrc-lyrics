using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace LrcLyrics.Models
{
    public class LyricsSubmission
    {
        [BsonId]
        public BsonObjectId BsonId { get; set; }

        [BsonElement("id")]
        public int Id { get; set; }

        [BsonElement("lyrics")]
        public Lyrics Lyrics { get; set; }

        [BsonElement("state")]
        public SubmissionState State { get; set; }

        [BsonElement("deniedReason")]
        public string DeniedReason { get; set; }

        [BsonElement("acceptedId")]
        public int? AcceptedId { get; set; }

        [BsonElement("editId")]
        public int? EditId { get; set; }

        [BsonElement("editBase")]
        public string EditBase { get; set; }

        [BsonElement("dateSubmitted")]
        public DateTime DateSubmitted { get; set; }

        [BsonElement("dateUpdated")]
        public DateTime DateUpdated { get; set; }

        [BsonElement("rawText")]
        public string RawText { get; set; }

        [BsonElement("requestToClose")]
        public int? RequestToClose { get; set; }

        [BsonElement("keys")]
        public List<string> Keys { get; set; } = new List<string>();

        [BsonIgnore]
        public string Url => @$"/Lyrics/Submissions/{Id}";
    }

    public enum SubmissionState
    {
        Pending = 0,
        Published = 1,
        Denied = 2
    }
}
