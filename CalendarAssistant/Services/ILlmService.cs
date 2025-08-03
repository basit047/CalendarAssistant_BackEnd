namespace CalendarAssistant.Services
{
    public interface ILlmService
    {
        Task<string> GetLLMResponse(string prompt);
    }
}
