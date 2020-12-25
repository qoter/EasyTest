using System;
using System.IO;
using System.Text;
using EasyTest.Utils;
using Xunit;

namespace EasyTest.Tests.Utils
{
    public class StreamExtensionsTests : IDisposable
    {
        private readonly string tempDirectory;

        public StreamExtensionsTests()
        {
            tempDirectory = Guid.NewGuid().ToString("N");
            Directory.CreateDirectory(tempDirectory);
        }

        public void Dispose()
        {
            if (tempDirectory != null && Directory.Exists(tempDirectory))
                Directory.Delete(tempDirectory, true);
        }
        
        [Fact]
        public void Read_FileWithBom_ReturnStringWithoutBom()
        {
            var filePath = Path.Combine(tempDirectory, "test.txt");
            File.WriteAllText(filePath, "Hello, World!", new UTF8Encoding(true));
            
            using var stream = File.OpenRead(filePath);

            var str = stream.ReadString();
            
            Assert.Equal("Hello, World!", str);
            Assert.True(stream.CanRead);
        }

        [Fact]
        public void Read_FromEmptyFile_ReturnEmptyStringAndNotCloseStream()
        {
            var filePath = Path.Combine(tempDirectory, "test.txt");
            using (File.Create(filePath)) { }
            using var stream = File.OpenRead(filePath);

            var str = stream.ReadString();
            
            Assert.Equal("", str);
            Assert.True(stream.CanRead);
        }
        
        [Fact]
        public void Read_FromFile_ReturnAllContentAndNotCloseStream()
        {
            var filePath = Path.Combine(tempDirectory, "test.txt");
            const string content = "hello\n\tworld!";
            File.WriteAllText(filePath, content);
            using var stream = File.OpenRead(filePath);

            var str = stream.ReadString();
            
            Assert.Equal(content, str);
            Assert.True(stream.CanRead);
        }

        [Fact]
        public void WriteManyTimes_SuccessWriteAndNotCloseStream()
        {
            var filePath = Path.Combine(tempDirectory, "test.txt");
            using (var stream = File.OpenWrite(filePath))
            {
                stream.WriteString("Hello\n");
                stream.WriteString("World");
            
                Assert.True(stream.CanWrite);
            }
            Assert.Equal("Hello\nWorld", File.ReadAllText(filePath));
        }
    }
}