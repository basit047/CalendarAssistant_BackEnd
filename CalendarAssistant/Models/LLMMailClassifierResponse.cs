namespace CalendarAssistant.Models
{
    public class LLMMailClassifierResponse
    {
        public bool IsEmailMeetingInvite { get; set; }
        public string BriefExplanation { get; set; }
        public string? ScheduledDateTime { get; set; }
    }

    public class LLMMailResponseForNonMeeting
    {
        public string Reply { get; set; }
        public string? Summary { get; set; }
    }
}
