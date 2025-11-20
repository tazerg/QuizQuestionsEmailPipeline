using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using QuizQuestions.Model;

namespace QuizQuestions.OpenAiProcessor
{
    public class OpenAiProcessor
    {
        private readonly HttpClient _httpClient;
        
        public OpenAiProcessor(string apiKey)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.openai.com/v1/")
            };
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        public async Task<ProcessedQuestion> ProcessEmailAsync(string universe, string subject, string body)
        {
            var systemPrompt = PromptTemplate.TEMPLATE.Replace("<UNIVERSE>", universe);
            var userContent = $"Subject: {subject}\nBody:\n{body}";

            var payload = new
            {
                model = "gpt-5.1",
                response_format = new { type = "json_object" },
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userContent }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var chatResponse = JsonSerializer.Deserialize<OpenAiChatResponse>(responseString);
            var resultJson = chatResponse?.Choices?[0]?.Message?.Content;

            if (string.IsNullOrWhiteSpace(resultJson))
                throw new Exception("Empty model response");

            var processed = JsonSerializer.Deserialize<ProcessedQuestion>(
                resultJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return processed;
        }
    }
}