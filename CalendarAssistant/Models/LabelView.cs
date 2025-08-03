namespace CalendarAssistant.Models
{
    public class LabelView
    {
        public string? Name { get; set; }
        public string? Color { get; set; }
        public string? Id { get; set; }
    }

    public class MailFilter
    {
        public string? From { get; set; }
        public string? Subject { get; set; }
        public string? Label { get; set; }
        public string? To { get; set; }
        public DateTime? Before { get; set; } = null;
        public DateTime? After { get; set; } = null;
        public bool HasAttachment { get; set; } = false;
        public int NumberOfMail { get; set; } = 10;
    }

    public class MailView
    {
        public string? Subject { get; set; }
        public string? ReceivedAt { get; set; }
        public string? From { get; set; }
        public string? Snippet { get; set; }
    }


}
