namespace CalendarAssistant.Models
{
    public class CalendarEvent
    {
        public string? Summary { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string[]? Attendees { get; set; }
    }

    public class CancelEvent
    {
        public string? EventId { get; set; }
    }
}
