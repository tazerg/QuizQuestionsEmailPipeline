using System.Diagnostics;

namespace QuizQuestions.Logger
{
    public static class Log
    {
        [Conditional("DEBUG")]
        public static void Debug(string tag, string message)
        {
            Console.WriteLine($"[{tag}]: {message}");
        }
    }
}