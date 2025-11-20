namespace QuizQuestions.OpenAiProcessor
{
    public class OpenAiChatResponse
    {
        public List<Choice> Choices { get; init; }

        public class Choice
        {
            public Message Message { get; init; }
        }

        public class Message
        {
            public string Role { get; set; }
            public string Content { get; init; }
        }
    }
}