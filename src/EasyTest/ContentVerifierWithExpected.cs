using System;
using System.IO;

namespace EasyTest
{
    public class ExpectedFileNotFoundException : FileNotFoundException
    {
        public override string Message => $"Don't worry! Expected file '{expectedFilePath}' not found, but we save actual file '${actualFilePath}'";
        
        private readonly string expectedFilePath;
        private readonly string actualFilePath;

        public ExpectedFileNotFoundException(string expectedFilePath, string actualFilePath)
        {
            this.expectedFilePath = expectedFilePath;
            this.actualFilePath = actualFilePath;
        }
    }
    public class ContentVerifierWithExpected<TExpected>
    {
        private readonly string directory;
        private readonly string actualFileName;
        private readonly Action<Stream> writeActual;
        private readonly string expectedFileName;
        private readonly Func<Stream, TExpected> readExpected;

        internal ContentVerifierWithExpected(
            string directory,
            string actualFileName,
            Action<Stream> writeActual,
            string expectedFileName,
            Func<Stream, TExpected> readExpected)
        {
            this.directory = directory;
            this.actualFileName = actualFileName;
            this.writeActual = writeActual;
            this.expectedFileName = expectedFileName;
            this.readExpected = readExpected;
        }
        
        public void Verify(Action<TExpected> assertion)
        {
            if (assertion == null) throw new ArgumentNullException(nameof(assertion));

            if (!Directory.Exists(directory)) 
                throw new DirectoryNotFoundException($"Directory '{directory}' not found.");
            
            var expectedFilePath = Path.Combine(directory, expectedFileName);
            var actualFilePath = Path.Combine(directory, actualFileName);

            if (!File.Exists(expectedFilePath))
            {
                SaveActual(actualFilePath);
                File.WriteAllText(expectedFilePath, "");
                PrintClickableCommands(directory, actualFilePath, expectedFilePath);
                
                throw new ExpectedFileNotFoundException(expectedFilePath, actualFilePath);
            }

            using var expectedFileStream = File.OpenRead(expectedFilePath);
            var expected = readExpected(expectedFileStream);

            try
            {
                assertion(expected);
            }
            catch
            {
                PrintClickableCommands(directory, actualFilePath, expectedFilePath);
                SaveActual(actualFilePath);
                throw;
            }
        }

        private void SaveActual(string actualFilePath)
        {
            if (File.Exists(actualFilePath))
                File.Delete(actualFilePath);
            using var actualFileStream = File.OpenWrite(actualFilePath);
            writeActual(actualFileStream);
        }

        private static void PrintClickableCommands(
            string testDirectory, 
            string actualFilePath, 
            string expectedFilePath)
        {
            testDirectory = Path.GetFullPath(testDirectory);
            actualFilePath = Path.GetFullPath(actualFilePath);
            expectedFilePath = Path.GetFullPath(expectedFilePath);

            Console.WriteLine(ClickableCommands.CreateNavigateCommand(testDirectory));
            Console.WriteLine(ClickableCommands.CreateViewDiffCommand(actualFilePath, expectedFilePath));
            Console.WriteLine(ClickableCommands.CreateAcceptDiffCommand(actualFilePath, expectedFilePath));
        }
    }
}