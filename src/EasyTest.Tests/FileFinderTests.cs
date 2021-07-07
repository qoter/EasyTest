using System;
using System.IO;
using System.Linq;
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
        [InlineData("*a.txt", new [] {"a.txt"}, new [] {"a.txt"})]
        [InlineData("*", new [] {"a.txt"}, new [] {"a.txt"})]
        [InlineData("a.txt|*b.txt", new [] {"bbb.txt"}, new [] {"bbb.txt"})]
        [InlineData("a.txt|*b.txt", new [] {"a.txt", "bbb.txt"}, new [] {"a.txt", "bbb.txt"})]
        [InlineData("*b.txt|a.txt", new [] {"a.txt", "bbb.txt"}, new [] {"bbb.txt", "a.txt"})]
        [InlineData("a.*|b.*", new [] {"a.a", "b.a", "b.b"}, new [] {"a.a", "b.a", "b.b"})]
        [InlineData("a.*", new [] { "b.b", "ac.a" }, new string[0])]
        public void FindPathToFileTest(string template, string[] files, string[] expectedFilePaths)
        {
            foreach (var file in files)
            {
                File.WriteAllText(Path.Combine(tempDirectory, file), file);
            }
            for (var i = 0; i < expectedFilePaths.Length; i++)
            {
                expectedFilePaths[i] = Path.GetFullPath(Path.Combine(tempDirectory, expectedFilePaths[i]));
            }
            
            var actualFilePaths = FileFinder.FindPaths(tempDirectory, template).ToList();

            Assert.Equal(expectedFilePaths, actualFilePaths);
        }
    }
}