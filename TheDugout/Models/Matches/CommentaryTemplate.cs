namespace TheDugout.Models.Matches
{
    public class CommentaryTemplate
    {
        public int Id { get; set; }
        public string EventTypeCode { get; set; } = string.Empty;
        public string OutcomeName { get; set; } = string.Empty;
        public string Template { get; set; } = string.Empty;
        public int EventOutcomeId { get; set; }
        public EventOutcome EventOutcome { get; set; } = null!;
    }
}
