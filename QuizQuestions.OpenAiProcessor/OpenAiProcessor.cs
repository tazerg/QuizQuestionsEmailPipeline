using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using QuizQuestions.Logger;
using QuizQuestions.Model;

namespace QuizQuestions.OpenAiProcessor
{
    public class OpenAiProcessor
    {
        private const string LOG_TAG = nameof(OpenAiProcessor);
        
        private readonly HttpClient _httpClient;
        
        public OpenAiProcessor(string apiKey)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.openai.com/v1/")
            };
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        public async Task<List<ProcessedQuestion>> ProcessEmailAsync(string universe, List<EmailForModeration> emails)
        {
            if (emails == null || emails.Count == 0)
                throw new ArgumentException("emails list is empty", nameof(emails));
            
            Log.Debug(LOG_TAG, $"Start batch process. Emails count: {emails.Count}");
            
            var systemPrompt = PromptTemplate.TEMPLATE.Replace("<UNIVERSE>", universe);
            var emailsJson = JsonSerializer.Serialize(emails);

            var payload = new
            {
                model = "gpt-4o-mini",
                //response_format = new { type = "json_object" },
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = emailsJson }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Log.Debug(LOG_TAG, "Send request");
            var response = await _httpClient.PostAsync("chat/completions", content);
            response.EnsureSuccessStatusCode();

            Log.Debug(LOG_TAG, "Wait response");
            var responseString = await response.Content.ReadAsStringAsync();
            
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var chatResponse = JsonSerializer.Deserialize<OpenAiChatResponse>(responseString, jsonOptions);
            
            Log.Debug(LOG_TAG, $"Response received. Usage info. " +
                               $"Prompt tokens: {chatResponse.Usages.PromptTokens}; " +
                               $"Completion tokens: {chatResponse.Usages.CompletionTokens}; " +
                               $"Total tokens: {chatResponse.Usages.TotalTokens}");
            
            var rateLimitInfo = ParseRateLimitInfo(response);
            Log.Debug(LOG_TAG, $"{rateLimitInfo}");
            
            var resultJson = chatResponse?.Choices?[0]?.Message?.Content;

            if (string.IsNullOrWhiteSpace(resultJson))
                throw new Exception("Empty model response");

            Log.Debug(LOG_TAG, "Deserialize result");
            var processedList = JsonSerializer.Deserialize<List<ProcessedQuestion>>(resultJson, jsonOptions);
            return processedList;
        }
        
        private OpenAiRateLimitInfo ParseRateLimitInfo(HttpResponseMessage response)
        {
            var headers = response.Headers;

            int? TryInt(string name)
            {
                if (headers.TryGetValues(name, out var values))
                {
                    var s = values.FirstOrDefault();
                    if (int.TryParse(s, out var v)) return v;
                }
                return null;
            }

            string TryString(string name)
            {
                if (headers.TryGetValues(name, out var values))
                    return values.FirstOrDefault();
                return null;
            }

            int? processingMs = null;
            var processingMsStr = TryString("openai-processing-ms");
            if (int.TryParse(processingMsStr, out var ms))
                processingMs = ms;

            return new OpenAiRateLimitInfo
            {
                LimitRequests = TryInt("x-ratelimit-limit-requests"),
                RemainingRequests = TryInt("x-ratelimit-remaining-requests"),
                ResetRequestsRaw = TryString("x-ratelimit-reset-requests"),

                LimitTokens = TryInt("x-ratelimit-limit-tokens"),
                RemainingTokens = TryInt("x-ratelimit-remaining-tokens"),
                ResetTokensRaw = TryString("x-ratelimit-reset-tokens"),

                ProcessingMs = processingMs,
                RequestId = TryString("X-Request-ID")
            };
        }
    }
}