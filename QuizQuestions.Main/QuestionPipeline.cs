using QuizQuestions.EmailClient;
using QuizQuestions.Json;
using QuizQuestions.Logger;

namespace QuizQuestions.Main
{
    public class QuestionPipeline
    {
        private const string LOG_TAG = nameof(QuestionPipeline);
        
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
            var emails = await _emailClient.GetUnprocessedEmailsAsync();
            var processedEmails = new List<EmailMessage>();
            
            foreach (var email in emails)
            {
                try
                {
                    var universe = DetectUniverseFromSubject(email.Subject);
                    if (universe == null)
                        continue;
                    
                    if (!CorrectUniverse(universe, requiredUniverse))
                        continue;

                    Log.Debug(LOG_TAG, "Send OpenAi prompt");
                    var result = await _openAiProcessor.ProcessEmailAsync(universe, email.Subject, email.BodyText);
                    Log.Debug(LOG_TAG, $"OpenAi status result {result.Status}");
                    if (result.Status == "accepted")
                    {
                        await _storage.AppendDataAsync(result);
                    }
                    else
                    {
                        Log.Debug(LOG_TAG, $"Reason: {result.RejectReason}. Details {result.Details}");
                    }

                    processedEmails.Add(email);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing email '{email.Subject}': {ex.Message}");
                }
            }
            
            Log.Debug(LOG_TAG, "Mark processed emails");
            await _emailClient.MarkAsProcessedAsync(processedEmails);
        }

        private string DetectUniverseFromSubject(string subject)
        {
            if (subject == null) 
                return null;

            if (subject.Contains("Harry Potter Quiz")) return "Harry Potter";
            if (subject.Contains("Middle Earth Quiz")) return "Lord of the Rings";
            if (subject.Contains("Witcher Quiz")) return "Witcher";

            // Или любые другие правила
            return null;
        }

        private bool CorrectUniverse(string universe, string requiredUniverse)
        {
            if (string.Equals(universe, "Harry Potter") && string.Equals(requiredUniverse, "hp")) return true;
            if (string.Equals(universe, "Lord of the Rings") && string.Equals(requiredUniverse, "lotr")) return true;
            if (string.Equals(universe, "Witcher") && string.Equals(requiredUniverse, "wq")) return true;

            return false;
        }
    }
}