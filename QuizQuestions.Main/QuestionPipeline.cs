using QuizQuestions.EmailClient;
using QuizQuestions.Json;
using QuizQuestions.Logger;
using QuizQuestions.Model;

namespace QuizQuestions.Main
{
    public class QuestionPipeline
    {
        private const string LOG_TAG = nameof(QuestionPipeline);

        private const int MAILS_COUNT = 20;
        
        private readonly IEmailClient _emailClient;
        private readonly OpenAiProcessor.OpenAiProcessor _openAiProcessor;
        private readonly JsonQuestionStorage _storage;

        public QuestionPipeline(IEmailClient emailClient, OpenAiProcessor.OpenAiProcessor openAiProcessor, JsonQuestionStorage storage)
        {
            _emailClient = emailClient;
            _openAiProcessor = openAiProcessor;
            _storage = storage;
        }

        public async Task ProcessAllInboxAsync(string requiredUniverse)
        {
            Log.Debug(LOG_TAG, "Collect emails");
            var subject = DetachSubjectFromUniverse(requiredUniverse);
            var emails = await _emailClient.GetUnprocessedEmailsAsync(subject, MAILS_COUNT);
            var moderationEmails = new List<EmailForModeration>(emails.Count);
            foreach (var email in emails)
            {
                moderationEmails.Add(new EmailForModeration
                {
                    Subject = email.Subject,
                    Body = email.BodyText
                });
            }
            
            try
            {
                var universe = DetectUniverseFromSubject(requiredUniverse);
                if (universe == null)
                    return;

                Log.Debug(LOG_TAG, "Send OpenAi prompt");
                var resultList = await _openAiProcessor.ProcessEmailAsync(universe, moderationEmails);
                foreach (var result in resultList)
                {
                    Log.Debug(LOG_TAG, $"OpenAi status result {result.Status}");
                    if (result.Status == "accepted")
                    {
                        await _storage.AppendDataAsync(result);
                    }
                    else
                    {
                        Log.Debug(LOG_TAG, $"Reason: {result.RejectReason}. Details {result.Details}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing email: {ex.Message}\n{ex.StackTrace}");
            }
            
            Log.Debug(LOG_TAG, "Mark processed emails");
            await _emailClient.MarkAsProcessedAsync(emails);
        }

        private string DetectUniverseFromSubject(string requiredUniverse)
        {
            if (string.Equals(requiredUniverse, "hp")) return "Harry Potter";
            if (string.Equals(requiredUniverse, "lotr")) return "Lord of the Rings";
            if (string.Equals(requiredUniverse, "wq")) return "Witcher";

            return null;
        }

        private string DetachSubjectFromUniverse(string requiredUniverse)
        {
            if (string.Equals(requiredUniverse, "hp")) return "Harry Potter Quiz";
            if (string.Equals(requiredUniverse, "lotr")) return "Middle Earth Quiz";
            if (string.Equals(requiredUniverse, "wq")) return "Witcher Quiz";

            return string.Empty;
        }
    }
}