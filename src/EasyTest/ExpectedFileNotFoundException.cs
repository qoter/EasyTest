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
}