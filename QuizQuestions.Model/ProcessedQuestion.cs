using System.Text.Json.Serialization;

namespace QuizQuestions.Model
{
    public class ProcessedQuestion
    {
        [JsonPropertyName("universe")]
        public string Universe { get; set; }
        [JsonPropertyName("source_language")]
        public string SourceLanguage { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("reject_reason")]
        public string RejectReason { get; set; }
        [JsonPropertyName("details")]
        public string Details { get; set; }
        [JsonPropertyName("difficulty_1_to_7")]
        public int? Difficulty1To7 { get; set; }
        [JsonPropertyName("question")]
        public LocalizedText Question { get; set; }
        [JsonPropertyName("answers")]
        public LocalizedAnswers Answers { get; set; }
        [JsonPropertyName("correct_answer_index")]
        public int? CorrectAnswerIndex { get; set; }
    }
}