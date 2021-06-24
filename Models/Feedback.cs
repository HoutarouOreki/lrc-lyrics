namespace LrcLyrics.Models
{
    public class Feedback
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Contact { get; set; }
        public SubmissionState State { get; set; }
    }
}
