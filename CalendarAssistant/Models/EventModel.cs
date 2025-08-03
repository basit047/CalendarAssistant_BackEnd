namespace CalendarAssistant.Models
{
    public class EventModel
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string? Title { get; set; }
        public string? ConflictingTitle { get; set; }
        public DateTime ConflictingStartDateTime { get; set; }
        public DateTime ConflictingEndDateTime { get; set; }
        public DateTime SuggestedStartDateTime { get; set; }
        public DateTime SuggestedEndDateTime { get; set; }


        public UpdateEvent UpdateEvent { get; set; }
        public SendEmailModel SendEmailModel { get; set; }
    }
}
