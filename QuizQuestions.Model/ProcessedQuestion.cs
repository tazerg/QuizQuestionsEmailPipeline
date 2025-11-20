namespace QuizQuestions.Model
{
    public class ProcessedQuestion
    {
        public string Universe { get; init; }
        public string SourceLanguage { get; init; }
        public string Status { get; init; }
        public string RejectReason { get; init; }
        public string Details { get; init; }
        public int? Difficulty1To7 { get; init; }
        public LocalizedText Question { get; init; }
        public LocalizedAnswers Answers { get; init; }
        public int? CorrectAnswerIndex { get; init; }
    }
}