namespace QuizQuestions.Main
{
    public class Args
    {
        public string Email { get; }
        public string EmailPass { get; }
        public string OpenAiKey { get; }
        public string SpreadsheetId { get; }
        public string SpreadsheetKeyPath { get; }
        public string JsonDirectory { get; }
        public string Universe { get; }

        public Args(string[] args)
        {
            var argDict = new Dictionary<string, string>();
            foreach (var arg in args)
            {
                var argPair = arg.Split('=');
                argDict.Add(argPair[0], argPair[1]);
            }

            Email = argDict["email"];
            EmailPass = argDict["emailPass"];
            OpenAiKey = argDict["openAiKey"];
            SpreadsheetId = argDict["spreadsheetId"];
            SpreadsheetKeyPath = argDict["spreadsheetKeyPath"];
            JsonDirectory = argDict["jsonDirectory"];
            Universe = argDict["universe"];
        }
    }
}