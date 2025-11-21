namespace QuizQuestions.OpenAiProcessor
{
    public class OpenAiRateLimitInfo
    {
        public int? LimitRequests { get; set; }
        public int? RemainingRequests { get; set; }
        public string ResetRequestsRaw { get; set; }

        public int? LimitTokens { get; set; }
        public int? RemainingTokens { get; set; }
        public string ResetTokensRaw { get; set; }

        public int? ProcessingMs { get; set; }
        public string RequestId { get; set; }

        public override string ToString()
        {
            return $"Current limit info: Limit requests: {LimitRequests}; Limit tokens {LimitTokens};\n" +
                   $"Remaining requests: {RemainingRequests}; Remaining tokens {RemainingTokens};\n" +
                   $"Reset requests: {ResetRequestsRaw}; Reset tokens: {ResetTokensRaw}";
        }
    }
}