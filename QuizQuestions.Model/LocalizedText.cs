using System.Text.Json.Serialization;

namespace QuizQuestions.Model
{
    public class LocalizedText
    {
        [JsonPropertyName("en")]
        public string En { get; set; }
        [JsonPropertyName("ru")]
        public string Ru { get; set; }
        [JsonPropertyName("de")]
        public string De { get; set; }
        [JsonPropertyName("fr")]
        public string Fr { get; set; }
    }   
}