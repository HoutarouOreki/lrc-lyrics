namespace LrcLyrics.BackEnd.Models
{
    public class LyricsSource
    {
        public SourceType Type { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
    }

    public enum SourceType
    {
        BasedOn = 0,
        Copied = 1,
    }
}