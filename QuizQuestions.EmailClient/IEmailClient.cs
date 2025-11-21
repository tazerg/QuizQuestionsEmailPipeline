namespace QuizQuestions.EmailClient
{
    public interface IEmailClient
    {
        Task<IReadOnlyList<EmailMessage>> GetUnprocessedEmailsAsync();
        Task MarkAsProcessedAsync(IReadOnlyList<EmailMessage> messages);
    }
}