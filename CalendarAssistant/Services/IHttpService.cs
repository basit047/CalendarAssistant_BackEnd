using CalendarAssistant.Models;

namespace CalendarAssistant.Services
{
    public interface IHttpService
    {
        Task<LLMMailClassifierResponse> GetEmailClassificationOllamaPostAsync(string url, string prompt, string model);
        Task<LLMMailResponseForNonMeeting> GetEmailResponseSummaryOllamaPostAsync(string url, string prompt, string model);
        Task<ClassifierResponse> PostAsyncToClassifier(string url, string mailSnippet);
    }
}
