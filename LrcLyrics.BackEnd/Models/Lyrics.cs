using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace LrcLyrics.BackEnd.Models
{
    public class Lyrics
    {
        [BsonId]
        public BsonObjectId BsonId { get; set; }

        [BsonElement("id")]
        public int Id { get; set; }

        [BsonElement("artist")]
        public string Artist { get; set; }

        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("creators")]
        public string Creators { get; set; }

        [BsonElement("musicUrl")]
        public string MusicUrl { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("lines")]
        public List<LyricLine> Lines { get; set; }

        [BsonElement("dateAccepted")]
        public DateTime DateAccepted { get; set; }

        [BsonElement("dateUpdated")]
        public DateTime DateUpdated { get; set; }

        [BsonIgnore]
        public string Url => @$"/Lyrics/{Id}/{Artist}/{Title}";

        public string GetLrcString()
        {
            var s = new StringBuilder();
            s.AppendLyricTag("ar", Artist);
            s.AppendLyricTag("ti", Title);
            s.AppendLyricTag("by", string.Join(", ", Creators));
            s.AppendLyricTag("re", "LRC Lyrics");
            foreach (var line in Lines)
                s.AppendLine(line.ToString());
            return s.ToString();
        }
    }
}
