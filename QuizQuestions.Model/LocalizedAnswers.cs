using System.Text.Json.Serialization;

namespace QuizQuestions.Model
{
    public class LocalizedAnswers
    {
        [JsonPropertyName("en")]
        public List<string> En { get; set; }
        [JsonPropertyName("ru")]
        public List<string> Ru { get; set; }
        [JsonPropertyName("de")]
        public List<string> De { get; set; }
        [JsonPropertyName("fr")]
        public List<string> Fr { get; set; }
    }
}