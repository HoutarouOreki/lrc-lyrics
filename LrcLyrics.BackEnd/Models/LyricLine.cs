using MongoDB.Bson.Serialization.Attributes;
using System;

namespace LrcLyrics.BackEnd.Models
{
    public class LyricLine
    {
        /// <summary>
        /// Time in hundredths of a second
        /// </summary>
        [BsonElement("time")]
        public int Time { get; set; }

        [BsonIgnore]
        public TimeSpan TimeSpan => TimeSpan.FromMilliseconds(Time * 10);

        [BsonElement("text")]
        public string Text { get; set; }

        public override string ToString() => $@"[{TimeString}]{Text}";

        public string TimeString => $@"{TimeSpan:mm\:ss\.ff}";
    }
}
