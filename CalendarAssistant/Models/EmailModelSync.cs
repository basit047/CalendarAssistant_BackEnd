namespace CalendarAssistant.Models
{
    public class EmailModelSync
    {
        public string? From { get; set; }
        public string? Snippet { get; set; }
        public bool IsMeetingInvite { get; set; }
        public bool HasConflict { get; set; }
        public DateTime? ReceivedAt { get; set; }
        public EventModel EventModel { get; set; }
        public CalendarEvent CalendarEvent { get; set; }
        public NonMeetingInviteResponse NonMeetingInviteResponse { get; set; }
        public string? Title { get; set; }
        public string? MessageId { get; set; }
        public string? Attendees { get; set; }
    }

    public class NonMeetingInviteResponse 
    {
        public string Summary { get; set; }
        public string ResponseSuggestedByLLM { get; set; }
    }
}
