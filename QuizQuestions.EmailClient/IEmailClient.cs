namespace QuizQuestions.EmailClient
{
    public interface IEmailClient
    {
        Task<IReadOnlyList<EmailMessage>> GetUnprocessedEmailsAsync(string subject, int mailsCount);
        Task MarkAsProcessedAsync(IReadOnlyList<EmailMessage> messages);
    }
}