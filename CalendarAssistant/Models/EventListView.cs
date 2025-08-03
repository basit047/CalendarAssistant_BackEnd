namespace CalendarAssistant.Models
{
    public class EventListView
    {
        public string? Attendees { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public int? DateTimeOffsetInHours { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public string? StartDay { get; set; }
        public string? EndDay { get; set; }
        public double? TotalDurationInMinutes { get; set; }
        public string? EventId { get; set; }
        public string? EventType { get; set; }
    }

    public class DayEvent
    {
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
    }
}
