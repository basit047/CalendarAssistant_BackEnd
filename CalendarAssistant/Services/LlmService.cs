using System.Text;
using System.Text.Json;

namespace CalendarAssistant.Services
{
    public class LlmService : ILlmService
    {

        private static readonly HttpClient client = new HttpClient { BaseAddress = new Uri("http://localhost:11434") };

        public async Task<string> GetLLMResponse(string prompt)
        {
            string model = "phi3";
            var requestBody = new
            {
                model = model,
                prompt = prompt,
                stream = false
            };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                // Send the POST request to the Ollama API
                HttpResponseMessage response = await client.PostAsync("/api/generate", content);
                if (response.IsSuccessStatusCode)
                {

                    string responseBody = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
                    Console.WriteLine("\nOllama Response:");
                    Console.WriteLine(jsonResponse.GetProperty("response").GetString());
                    return jsonResponse.GetProperty("response").GetString()!;
                }
                else
                {
                    Console.WriteLine("Error in communication with the API.");
                    return "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                throw ex;
            }
        }
    }
}
