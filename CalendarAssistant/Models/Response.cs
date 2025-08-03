namespace CalendarAssistant.Models
{
    public class Response
    {
        public string? Status { get; set; }
        public string? Message { get; set; }

    }

    public enum MeetingStatus
    {
        confirmed = 0,
        tentative = 1,
        cancelled = 2,
        rescheduled = 3
    }
}
