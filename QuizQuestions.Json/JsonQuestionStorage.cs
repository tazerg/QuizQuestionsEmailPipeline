using System.Text.Json;
using QuizQuestions.Model;

namespace QuizQuestions.Json
{
    public class JsonQuestionStorage
    {
        private const string FILE_NAME_PREFIX = "questions_d";
        private const string FILE_EXTENSION = ".json";
        
        private readonly JsonSerializerOptions _options;
        
        private readonly string _directory;

        public JsonQuestionStorage(string directory)
        {
            _directory = directory;

            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            Directory.CreateDirectory(_directory);
        }

        public async Task AppendDataAsync(ProcessedQuestion question)
        {
            if (question.Difficulty1To7 == null)
                throw new ArgumentException("Question must have Difficulty1To7");

            var d = question.Difficulty1To7.Value;
            if (d < 1 || d > 7)
                throw new ArgumentOutOfRangeException(nameof(question.Difficulty1To7), "Difficulty must by in range from 1 to 7");

            var filePath = GetFilePath(d);
            var currentQuestions = await GetCurrentProcessedQuestions(filePath);
            currentQuestions.Add(question);

            var newJson = JsonSerializer.Serialize(currentQuestions, _options);
            await File.WriteAllTextAsync(filePath, newJson);
        }
        
        public List<ProcessedQuestion> LoadAllReviewed()
        {
            var result = new List<ProcessedQuestion>();
            for (var d = 1; d <= 7; d++)
            {
                var filePath = GetFilePath(d);
                if (!File.Exists(filePath)) 
                    continue;

                var json = File.ReadAllText(filePath);
                var list = JsonSerializer.Deserialize<List<ProcessedQuestion>>(json, _options);
                result.AddRange(list.Where(q => q.Status == "accepted"));
            }

            return result;
        }

        private async Task<List<ProcessedQuestion>> GetCurrentProcessedQuestions(string difficultyFilePath)
        {
            if (!File.Exists(difficultyFilePath)) 
                return new List<ProcessedQuestion>();
            
            var json = await File.ReadAllTextAsync(difficultyFilePath);
            return JsonSerializer.Deserialize<List<ProcessedQuestion>>(json, _options);
        }

        private string GetFilePath(int difficulty)
        {
            return Path.Combine(_directory, $"{FILE_NAME_PREFIX}{difficulty}{FILE_EXTENSION}");
        }
    }
}