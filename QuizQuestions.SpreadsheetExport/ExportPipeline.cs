using QuizQuestions.Json;

namespace QuizQuestions.SpreadsheetExport
{
    public class ExportPipeline
    {
        private readonly JsonQuestionStorage _storage;
        private readonly GoogleSheetsExporter _sheetsExporter;

        public ExportPipeline(JsonQuestionStorage storage, GoogleSheetsExporter sheetsExporter)
        {
            _storage = storage;
            _sheetsExporter = sheetsExporter;
        }

        public async Task ExportReviewedToSheetsAsync()
        {
            var questions = _storage.LoadAllReviewed();
            await _sheetsExporter.ExportAsync(questions);
        }
    }
}