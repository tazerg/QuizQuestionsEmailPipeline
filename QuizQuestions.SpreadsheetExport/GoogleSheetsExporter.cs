using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using QuizQuestions.Model;

namespace QuizQuestions.SpreadsheetExport
{
    public class GoogleSheetsExporter
    {
        private readonly SheetsService _service;
        
        private readonly string _spreadsheetId;
        
        public GoogleSheetsExporter(string spreadsheetId, string keyFilePath)
        {
            _spreadsheetId = spreadsheetId;

            GoogleCredential credential;
            using (var stream = new FileStream(keyFilePath, FileMode.Open, FileAccess.Read))
            {
                credential = CredentialFactory
                    .FromStream<ServiceAccountCredential>(stream)
                    .ToGoogleCredential()
                    .CreateScoped(SheetsService.Scope.Spreadsheets);
            }

            _service = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "QuizQuestionImporter"
            });
        }
        
        public async Task ExportAsync(List<ProcessedQuestion> questions)
        {
            var enSheet = BuildQuestionSheet(questions, "en");
            var ruSheet = BuildQuestionSheet(questions, "ru");
            var deSheet = BuildQuestionSheet(questions, "de");
            var frSheet = BuildQuestionSheet(questions, "fr");

            var qLocSheet = BuildQuestionLocalizationSheet(questions);
            var a1LocSheet = BuildAnswerLocalizationSheet(questions, 0);
            var a2LocSheet = BuildAnswerLocalizationSheet(questions, 1);
            var a3LocSheet = BuildAnswerLocalizationSheet(questions, 2);

            await WriteSheetAsync("Questions_en", enSheet);
            await WriteSheetAsync("Questions_ru", ruSheet);
            await WriteSheetAsync("Questions_de", deSheet);
            await WriteSheetAsync("Questions_fr", frSheet);

            await WriteSheetAsync("Questions_localization", qLocSheet);
            await WriteSheetAsync("Answer1_localization", a1LocSheet);
            await WriteSheetAsync("Answer2_localization", a2LocSheet);
            await WriteSheetAsync("Answer3_localization", a3LocSheet);
        }
        
        private IList<IList<object>> BuildQuestionSheet(List<ProcessedQuestion> questions, string lang)
        {
            var rows = new List<IList<object>>
            {
                new List<object> { "Question", "CorrectAnswerIndex", "Answer1", "Answer2", "Answer3" }
            };

            foreach (var question in questions)
            {
                var questionText = GetQuestionByLang(question, lang);
                var answers = GetAnswersByLang(question, lang);
                if (answers.Count < 3) 
                    continue;

                rows.Add(new List<object>
                {
                    questionText,
                    question.CorrectAnswerIndex,
                    answers[0],
                    answers[1],
                    answers[2]
                });
            }

            return rows;
        }
        
        private IList<IList<object>> BuildQuestionLocalizationSheet(List<ProcessedQuestion> questions)
        {
            var rows = new List<IList<object>>
            {
                new List<object> { "english", "russian", "german", "french" }
            };

            foreach (var q in questions)
            {
                rows.Add(new List<object>
                {
                    q.Question.En,
                    q.Question.Ru,
                    q.Question.De,
                    q.Question.Fr
                });
            }

            return rows;
        }
        
        private IList<IList<object>> BuildAnswerLocalizationSheet(List<ProcessedQuestion> questions, int answerIndex)
        {
            var rows = new List<IList<object>>
            {
                new List<object> { "english", "russian", "german", "french" }
            };

            foreach (var q in questions)
            {
                var en = q.Answers.En.Count > answerIndex ? q.Answers.En[answerIndex] : "";
                var ru = q.Answers.Ru.Count > answerIndex ? q.Answers.Ru[answerIndex] : "";
                var de = q.Answers.De.Count > answerIndex ? q.Answers.De[answerIndex] : "";
                var fr = q.Answers.Fr.Count > answerIndex ? q.Answers.Fr[answerIndex] : "";

                rows.Add(new List<object> { en, ru, de, fr });
            }

            return rows;
        }
        
        private async Task WriteSheetAsync(string sheetName, IList<IList<object>> rows)
        {
            var clearReq = new ClearValuesRequest();
            var clear = _service.Spreadsheets.Values.Clear(clearReq, _spreadsheetId, $"{sheetName}!A:Z");
            await clear.ExecuteAsync();

            var valueRange = new ValueRange { Values = rows };
            var appendReq = _service.Spreadsheets.Values.Update(valueRange, _spreadsheetId, $"{sheetName}!A1");
            appendReq.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            await appendReq.ExecuteAsync();
        }

        private string GetQuestionByLang(ProcessedQuestion question, string lang)
        {
            switch (lang)
            {
                case "en":
                    return question.Question.En;
                case "ru":
                    return question.Question.Ru;
                case "de":
                    return question.Question.De;
                case "fr":
                    return question.Question.Fr;
                default:
                    return question.Question.En;
            }
        }


        private List<string> GetAnswersByLang(ProcessedQuestion question, string lang)
        {
            switch (lang)
            {
                case "en":
                    return question.Answers.En;
                case "ru":
                    return question.Answers.Ru;
                case "de":
                    return question.Answers.De;
                case "fr":
                    return question.Answers.Fr;
                default:
                    return question.Answers.En;
            }
        }
    }
}