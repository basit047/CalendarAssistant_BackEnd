using CalendarAssistant.Models;

namespace CalendarAssistant.Services
{
    public interface IGoogleGmailService
    {
        Task<List<LabelView>> GetGmailLabels(string? accessToken);
        Task<List<MailView>> GetMails(MailFilter filters, string? accessToken);
        Task<bool> SendEmail(SendEmailModel sendEmailModel, string? accessToken);
        Task<List<EmailModelSync>> CheckForNewEmails(EmailSyncModel emailSyncModel, string? accessToken, string? idToken);
        DateTime GetLastSyncDate(string email);
    }
}
