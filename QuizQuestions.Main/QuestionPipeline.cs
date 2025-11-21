using QuizQuestions.EmailClient;
using QuizQuestions.Json;

namespace QuizQuestions.Main
{
    public class QuestionPipeline
    {
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

                    var result = await _openAiProcessor.ProcessEmailAsync(universe, email.Subject, email.BodyText);
                    if (result.Status == "accepted")
                    {
                        await _storage.AppendDataAsync(result);
                    }

                    processedEmails.Add(email);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing email '{email.Subject}': {ex.Message}");
                }
            }
            
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