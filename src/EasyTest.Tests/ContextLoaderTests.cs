using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
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
            var filePath = Path.GetFullPath(Path.Combine(tempDirectory, "file.txt"));
            File.WriteAllText(filePath, "Hello!", Encoding.UTF8);
            
            using var context = ContextLoader
                .For<InjectPathContext>()
                .WithDeserializer(s => new StreamReader(s, Encoding.UTF8).ReadToEnd())
                .LoadFromDirectory(tempDirectory);
            
            Assert.Equal(filePath, context.FilePath);
        }
        
        private class LoadOptionalStringContext : TestContext
        {
            [TestFile("file.txt", Optional = true)]
            public string StringProperty { get; private set; }
        }
        
        [Fact]
        public void LoadOptionalString_FileExists_Success()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "file.txt"), "Hello!", Encoding.UTF8);
            
            using var context = ContextLoader
                .For<LoadOptionalStringContext>()
                .WithDeserializer(s => new StreamReader(s, Encoding.UTF8).ReadToEnd())
                .LoadFromDirectory(tempDirectory);
            
            Assert.Equal("Hello!", context.StringProperty);
        }
        
        [Fact]
        public void LoadOptionalString_FileNotExists_Success()
        {
            using var context = ContextLoader
                .For<LoadOptionalStringContext>()
                .WithDeserializer(s => new StreamReader(s, Encoding.UTF8).ReadToEnd())
                .LoadFromDirectory(tempDirectory);
            
            Assert.Null(context.StringProperty);
        }
        
        private class LoadGlobalStringContext : TestContext
        {
            [TestFile("file.txt", Global = true)]
            public string StringProperty { get; private set; }
        }

        [Fact] 
        public void LoadGlobalString_FileInCurrentDirectory_Success()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "file.txt"), "Hello!", Encoding.UTF8);
            
            using var context = ContextLoader
                .For<LoadGlobalStringContext>()
                .WithDeserializer(s => new StreamReader(s, Encoding.UTF8).ReadToEnd())
                .LoadFromDirectory(tempDirectory);
            
            Assert.Equal("Hello!", context.StringProperty);
        }
        
        [Fact] 
        public void LoadGlobalString_FileInParentDirectory_Success()
        {
            var parentDirectory = tempDirectory;
            var contextDirectory = Path.Combine(parentDirectory, "context_directory");
            Directory.CreateDirectory(contextDirectory);

            File.WriteAllText(Path.Combine(parentDirectory, "file.txt"), "Hello!", Encoding.UTF8);
            
            using var context = ContextLoader
                .For<LoadGlobalStringContext>()
                .WithDeserializer(s => new StreamReader(s, Encoding.UTF8).ReadToEnd())
                .LoadFromDirectory(contextDirectory);
            
            Assert.Equal("Hello!", context.StringProperty);
        }
        
        [Fact] 
        public void LoadGlobalString_FileInAncestor20Directory_Success()
        {
            var ancestorDirectory = tempDirectory;
            var currentContextDirectory = ancestorDirectory;
            for (var i = 0; i < 20; i++)
            {
                currentContextDirectory = Path.Combine(currentContextDirectory, $"dir_{i}");
                Directory.CreateDirectory(currentContextDirectory);
            }

            
            File.WriteAllText(Path.Combine(tempDirectory, "file.txt"), "Hello!", Encoding.UTF8);
            
            using var context = ContextLoader
                .For<LoadGlobalStringContext>()
                .WithDeserializer(s => new StreamReader(s, Encoding.UTF8).ReadToEnd())
                .LoadFromDirectory(currentContextDirectory);
            
            Assert.Equal("Hello!", context.StringProperty);
        }
        
        private class DeserializerNotExistsContext : TestContext
        {
            [TestFile("file.xml")]
            public XDocument Xml { get; private set; }
        }
        
        [Fact] 
        public void DeserializerNotExists_ThrowsAndFreeResources()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "file.xml"), "<xml />", Encoding.UTF8);

            Assert.Throws<InvalidOperationException>(() => ContextLoader
                .For<DeserializerNotExistsContext>()
                .LoadFromDirectory(tempDirectory));
        }

        private class FileNotFoundContext : TestContext
        {
            [TestFile("a.txt")]
            public string A { get; private set; }
            
            [TestFile("b.txt")]
            public string B { get; private set; }
            
            [TestFile("c.txt")]
            public string C { get; private set; }
        }
        
        [Fact] 
        public void FileNotFound_ThrowsAndFreeResources()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "a.txt"), "AAA", Encoding.UTF8);
            File.WriteAllText(Path.Combine(tempDirectory, "c.txt"), "CCC", Encoding.UTF8);

            Assert.Throws<FileNotFoundException>(() => ContextLoader
                .For<FileNotFoundContext>()
                .WithDeserializer(s => new StreamReader(s, Encoding.UTF8).ReadToEnd())
                .LoadFromDirectory(tempDirectory));
        }

        private class DifferentDeserializersContext : TestContext
        {
            [TestFile("file.xml")]
            public XDocument Xml { get; private set; }
            
            [TestFile("array.txt")]
            public int[] Array { get; private set; }
            
            [TestFile("bytes.bin")]
            public byte[] Bytes { get; private set; }
        }
        
        [Fact] 
        public void DifferentDeserializers_Success()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "file.xml"), "<xml />", Encoding.UTF8);
            File.WriteAllText(Path.Combine(tempDirectory, "array.txt"), "1,2,3", Encoding.UTF8);
            File.WriteAllBytes(Path.Combine(tempDirectory, "bytes.bin"), new byte[] {1, 2, 3});

            using var context = ContextLoader
                .For<DifferentDeserializersContext>()
                .WithDeserializer(XDocument.Load)
                .WithDeserializer(s => new StreamReader(s).ReadToEnd().Split(",").Select(int.Parse).ToArray())
                .WithDeserializer(s =>
                {
                    var ms = new MemoryStream();
                    s.CopyTo(ms);
                    return ms.ToArray();
                })
                .LoadFromDirectory(tempDirectory);
            
            Assert.Equal(context.Xml.Root.Name.LocalName, "xml");
            Assert.Equal(context.Array, new [] {1, 2, 3});
            Assert.Equal(context.Bytes, new byte[] {1, 2, 3});
        }

    }
}