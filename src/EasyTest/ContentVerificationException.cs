using System;
using System.IO;

namespace EasyTest
{
    public class ContentVerificationException : Exception
    {
        public override string Message => message;
        private readonly string message;

        internal ContentVerificationException(
            Exception innerException,
            string expectedFilePath,
            string actualFilePath,
            string testDirectory)
        {
            message = CreateMessage(innerException, actualFilePath, expectedFilePath, testDirectory);
        }

        private static string CreateMessage(
            Exception innerException,
            string actualFilePath,
            string expectedFilePath, 
            string testDirectory)
        {
            return $@"Verification failed:
{innerException.Message}

Available commands:
{ClickableCommands.CreateNavigateCommand(testDirectory)}
{ClickableCommands.CreateViewDiffCommand(actualFilePath, expectedFilePath)}
{ClickableCommands.CreateAcceptDiffCommand(actualFilePath, expectedFilePath)}
";
        }
    }
}