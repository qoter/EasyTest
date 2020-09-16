using System;
using System.IO;
using Xunit;

namespace EasyTest.Tests
{
    public class FileFinderTests : IDisposable
    {
        private readonly string tempDirectory;

        public FileFinderTests()
        {
            tempDirectory = Guid.NewGuid().ToString("N");
            Directory.CreateDirectory(tempDirectory);
        }

        public void Dispose()
        {
            if (tempDirectory != null && Directory.Exists(tempDirectory))
                Directory.Delete(tempDirectory, true);
        }

        
        [Theory]
        [InlineData("*a.txt", new [] {"a.txt"}, "a.txt")]
        [InlineData("*", new [] {"a.txt"}, "a.txt")]
        [InlineData("a.txt|*b.txt", new [] {"bbb.txt"}, "bbb.txt")]
        [InlineData("a.txt|*b.txt", new [] {"a.txt", "bbb.txt"}, "a.txt")]
        [InlineData("*b.txt|a.txt", new [] {"a.txt", "bbb.txt"}, "bbb.txt")]
        [InlineData("a.*|b.*", new [] {"a.a", "b.a", "b.b"}, "a.a")]
        public void FindPathToFileTest(string template, string[] files, string expectedFile)
        {
            foreach (var file in files)
            {
                File.WriteAllText(Path.Combine(tempDirectory, file), file);
            }
            var expectedFilePath = Path.GetFullPath(Path.Combine(tempDirectory, expectedFile));
            
            var actualFilePath = FileFinder.FindPathToFile(tempDirectory, template);

            Assert.Equal(expectedFilePath, actualFilePath);
        }
    }
}