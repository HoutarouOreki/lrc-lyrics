namespace LrcLyrics.Models
{
    public class LyricsSource
    {
        private const int truncated_url_length = 30;

        public SourceType Type { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public bool IsEmpty => string.IsNullOrWhiteSpace(Name) && string.IsNullOrWhiteSpace(Url) && string.IsNullOrWhiteSpace(Description);

        public string GetTruncatedUrl()
        {
            var url = Url.Replace("https://www.", "", System.StringComparison.OrdinalIgnoreCase).Replace("http://www.", "", System.StringComparison.OrdinalIgnoreCase).Replace("https://", "", System.StringComparison.OrdinalIgnoreCase).Replace("http://", "", System.StringComparison.OrdinalIgnoreCase);
            if (url.Length > truncated_url_length)
                url = $"{url.Substring(0, truncated_url_length - 2)}...";
            return url;
        }
    }

    public enum SourceType
    {
        BasedOn = 0,
        Copied = 1,
    }
}