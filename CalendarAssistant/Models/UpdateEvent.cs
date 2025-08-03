using Google.Apis.Calendar.v3.Data;

namespace CalendarAssistant.Models
{
    public class UpdateEvent
    {
        public string? EventId { get; set; }
        public Event? EventObj { get; set; }
        public List<string?> Attendees { get; set; }
    }
}
