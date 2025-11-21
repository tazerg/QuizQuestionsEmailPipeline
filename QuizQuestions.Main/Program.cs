using QuizQuestions.EmailClient;
using QuizQuestions.Json;
using QuizQuestions.SpreadsheetExport;

namespace QuizQuestions.Main
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== Quiz Email Processor ===");
            
            var parsedArgs = new Args(args);
            var emailClient = new GmailClient(parsedArgs.Email, parsedArgs.EmailPass);
            var openAiClient = new OpenAiProcessor.OpenAiProcessor(parsedArgs.OpenAiKey);
            var storage = new JsonQuestionStorage(parsedArgs.JsonDirectory);
            var sheetsExporter = new GoogleSheetsExporter(parsedArgs.SpreadsheetId, parsedArgs.SpreadsheetKeyPath);

            var questionPipeline = new QuestionPipeline(emailClient, openAiClient, storage);
            var exportPipeline = new ExportPipeline(storage, sheetsExporter);

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Action:");
                Console.WriteLine("1 — Collect and process");
                Console.WriteLine("2 — Export");
                Console.WriteLine("Q — Quit");
                Console.Write("> ");
                
                var key = Console.ReadKey(intercept: true);
                Console.WriteLine();
                
                if (key.Key == ConsoleKey.D1 || key.Key == ConsoleKey.NumPad1)
                {
                    await RunProcessEmailsAsync(questionPipeline, parsedArgs.Universe);
                    continue;
                }
                
                if (key.Key == ConsoleKey.D2 || key.Key == ConsoleKey.NumPad2)
                {
                    await RunExportToSheetsAsync(exportPipeline);
                    continue;
                }
                
                if (key.Key == ConsoleKey.Q || key.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("Quit...");
                    break;
                }
                
                
                Console.WriteLine("Unsupported command!");
            }
        }
        
        private static async Task RunProcessEmailsAsync(QuestionPipeline pipeline, string universe)
        {
            Console.WriteLine("=== Processing new emails ===");
            try
            {
                await pipeline.ProcessAllInboxAsync(universe);
                Console.WriteLine("Done.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception");
                Console.WriteLine(ex);
            }
        }
        
        private static async Task RunExportToSheetsAsync(ExportPipeline exportPipeline)
        {
            Console.WriteLine("=== Export ===");
            try
            {
                await exportPipeline.ExportReviewedToSheetsAsync();
                Console.WriteLine("Done.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception");
                Console.WriteLine(ex);
            }
        }
    }
}