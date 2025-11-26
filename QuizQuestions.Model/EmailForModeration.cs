using System.Text.Json.Serialization;

namespace QuizQuestions.Model
{
    public class EmailForModeration
    {
        [JsonPropertyName("subject")]
        public string Subject { get; set; }
        [JsonPropertyName("body")]
        public string Body { get; set; }
    }
}