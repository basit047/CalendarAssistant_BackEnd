namespace CalendarAssistant.Models
{
    public class SendEmailModel
    {
        public string? To { get; set; }
        public string? Subject { get; set; }
        public string? Message { get; set; }
        public string? MessageId { get; set; }
    }

    public class UserAuthentication
    {
        public string? Message { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? Picture { get; set; }
        public string? TimeZone { get; set; }
        public string? AccessToken { get; set; }
        public string? IdToken { get; set; }

    }

    public class TokenRequest
    {
        public string? Token { get; set; }
    }
}
