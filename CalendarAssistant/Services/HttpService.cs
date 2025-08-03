using CalendarAssistant.Models;
using System.Text;
using System.Text.Json;

namespace CalendarAssistant.Services
{
    public class HttpService : IHttpService
    {
        public async Task<LLMMailClassifierResponse> GetEmailClassificationOllamaPostAsync(string url, string mailSnippet, string llmModelToUse)
        {
            try
            {
                string briefExplanation = "";

                var client = new HttpClient()
                {
                    Timeout = TimeSpan.FromMinutes(5)
                };

                var prompt = $@"You are a classification assistant.\nClassify the following email reply as a meeting invite or not, if its an email also find the date & time if there exists.Reply: {mailSnippet}\nRespond in this format:\nYes or No: <Your answer>\nReason: <Brief explanation>\nMentioned Date & Time (if found): <date time>";

                var json = $@"{{
                        ""model"": ""{llmModelToUse}"",
                        ""prompt"": ""{prompt}""
                    }}";

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                string responseString = await response.Content.ReadAsStringAsync();
                using var stream = await response.Content.ReadAsStreamAsync();
                using var reader = new System.IO.StreamReader(stream);

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        try
                        {
                            using var doc = JsonDocument.Parse(line);
                            if (doc.RootElement.TryGetProperty("response", out var responseText))
                                briefExplanation += responseText.GetString()?.ToLower();

                        }
                        catch (JsonException ex)
                        {
                            Console.WriteLine($"Invalid JSON: {ex.Message}");
                        }
                    }
                }

                var model = new LLMMailClassifierResponse();

                if (!string.IsNullOrEmpty(briefExplanation) && briefExplanation.Contains(":"))
                {
                    var splittedResponse = briefExplanation.Split(":");
                    if (splittedResponse != null && splittedResponse.Count() > 1)
                    {
                        model.IsEmailMeetingInvite = splittedResponse[0].ToLower() == "yes";
                        model.BriefExplanation = $"{splittedResponse[1]} - {splittedResponse[2] ?? ""}";
                        model.ScheduledDateTime = splittedResponse.Length > 2 ? splittedResponse[3] ?? "" : "";
                    }
                }

                return model;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<LLMMailResponseForNonMeeting> GetEmailResponseSummaryOllamaPostAsync(string url, string mailSnippet, string llmModelToUse)
        {
            try
            {
                string briefExplanation = "";

                var client = new HttpClient()
                {
                    Timeout = TimeSpan.FromMinutes(5)
                };

                var prompt = $@"You are an AI assistant that helps draft professional email reply and Summary.\nBelow is an email message. Suggest one appropriate reply and extract summary to the following email please add -------- between the reply and summary. Reply should be clear, concise, and suitable for replying directly to the original message.\nEmail:\n{mailSnippet}\nSuggested Replies:\nSuggested Summary";

                var json = $@"{{
                        ""model"": ""{llmModelToUse}"",
                        ""prompt"": ""{prompt}""
                    }}";

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                using var stream = await response.Content.ReadAsStreamAsync();
                string responseString = await response.Content.ReadAsStringAsync();
                using var reader = new System.IO.StreamReader(stream);

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        try
                        {
                            using var doc = JsonDocument.Parse(line);
                            if (doc.RootElement.TryGetProperty("response", out var responseText))
                                briefExplanation += responseText.GetString()?.ToLower();

                        }
                        catch (JsonException ex)
                        {
                            Console.WriteLine($"Invalid JSON: {ex.Message}");
                        }
                    }
                }

                var model = new LLMMailResponseForNonMeeting();

                if (!string.IsNullOrEmpty(briefExplanation) && briefExplanation.Contains(":"))
                {
                    var splittedResponse = briefExplanation.Split("summary:");
                    if (splittedResponse != null && splittedResponse.Count() > 1)
                    {
                        model.Reply = splittedResponse[0]?.Trim() ?? "";
                        model.Summary = splittedResponse[1]?.Trim() ?? "";
                    }
                }

                return model;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<ClassifierResponse> PostAsyncToClassifier(string url, string mailSnippet)
        {
            try
            {
               
                var client = new HttpClient()
                {
                    Timeout = TimeSpan.FromMinutes(2)
                };


                var json = $@"{{
                        ""text"": ""{mailSnippet}""
                    }}";
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                string result = await response.Content.ReadAsStringAsync();
                var model = JsonSerializer.Deserialize<ClassifierResponse>(result);
                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
