using System.IO;

namespace EasyTest
{
    public class ExpectedFileNotFoundException : FileNotFoundException
    {
        public override string Message => message;

        private readonly string message;
        
        internal ExpectedFileNotFoundException(string expectedFilePath, string actualFilePath, string testDirectory)
        {
            message = CreateMessage(expectedFilePath, actualFilePath, testDirectory);
        }

        private static string CreateMessage(string expectedFilePath, string actualFilePath, string testDirectory)
        {
            return $@"Don't worry! Expected file '{Path.GetFileName(expectedFilePath)}' not found, but we save actual file '{Path.GetFileName(actualFilePath)}'

Available commands:
{ClickableCommands.CreateNavigateCommand(testDirectory)}
{ClickableCommands.CreateAcceptDiffCommand(actualFilePath, expectedFilePath)}
";
        }
    }
}