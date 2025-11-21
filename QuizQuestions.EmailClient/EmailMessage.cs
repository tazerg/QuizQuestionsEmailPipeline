using MailKit;

namespace QuizQuestions.EmailClient
{
    public class EmailMessage
    {
        public UniqueId UniqueId { get; init; }
        public string Subject { get; init; }
        public string BodyText { get; init; }
    }
}