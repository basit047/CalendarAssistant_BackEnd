namespace CalendarAssistant.Services
{
    public class GoogleCalendarSettings : IGoogleCalendarSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string[] Scope { get; set; }
        public string ApplicationName { get; set; }
        public string User { get; set; }
        public string CalendarId { get; set; }
    }

    public interface IGoogleCalendarSettings
    {
        string ClientId { get; set; }
        string ClientSecret { get; set; }
        string[] Scope { get; set; }
        string ApplicationName { get; set; }
        string User { get; set; }
        string CalendarId { get; }
    }
}
