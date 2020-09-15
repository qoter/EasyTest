using System;
using System.IO;
using System.Text;
using Xunit;

namespace EasyTest.Tests
{
    public class ContextLoaderTests : IDisposable
    {
        private readonly string tempDirectory;

        public ContextLoaderTests()
        {
            tempDirectory = Guid.NewGuid().ToString("N");
            Directory.CreateDirectory(tempDirectory);
        }

        public void Dispose()
        {
            if (tempDirectory != null && Directory.Exists(tempDirectory))
                Directory.Delete(tempDirectory, true);
        }

        private class LoadRequiredStringContext : TestContext
        {
            [TestFile("file.txt")]
            public string StringProperty { get; private set; }
        }
        
        [Fact]
        public void LoadRequiredString_FileExists_Success()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "file.txt"), "Hello!", Encoding.UTF8);
            
            using var context = ContextLoader
                .For<LoadRequiredStringContext>()
                .WithDeserializer(s => new StreamReader(s, Encoding.UTF8).ReadToEnd())
                .LoadFromDirectory(tempDirectory);
            
            Assert.Equal("Hello!", context.StringProperty);
        }

        [Fact]
        public void LoadRequiredString_FileNotExists_Throws()
        {
            var context = ContextLoader
                .For<LoadRequiredStringContext>()
                .WithDeserializer(s => new StreamReader(s, Encoding.UTF8).ReadToEnd());
            
            Assert.Throws<FileNotFoundException>(() => context.LoadFromDirectory(tempDirectory));
        }

        private class InjectPathContext : TestContext
        {
            [TestFile("file.txt", InjectPath = true)]
            public string FilePath { get; private set; }
        }

        [Fact]
        public void InjectPath()
        {
            var filePath = Path.Combine(tempDirectory, "file.txt");
            File.WriteAllText(filePath, "Hello!", Encoding.UTF8);
            
            using var context = ContextLoader
                .For<InjectPathContext>()
                .WithDeserializer(s => new StreamReader(s, Encoding.UTF8).ReadToEnd())
                .LoadFromDirectory(tempDirectory);
            
            Assert.Equal(filePath, context.FilePath);
        }
    }
}