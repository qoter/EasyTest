using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using EasyTest.Utils;
using Xunit;

namespace EasyTest.Tests
{
    public class ContentLoaderTests : IDisposable
    {
        private readonly string tempDirectory;

        public ContentLoaderTests()
        {
            tempDirectory = Guid.NewGuid().ToString("N");
            Directory.CreateDirectory(tempDirectory);
        }

        public void Dispose()
        {
            if (tempDirectory != null && Directory.Exists(tempDirectory))
                Directory.Delete(tempDirectory, true);
        }

        private class LoadRequiredStringContent : TestContent
        {
            [FileContent("file.txt")]
            public string StringProperty { get; private set; }
        }
        
        [Fact]
        public void LoadRequiredString_FileExists_Success()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "file.txt"), "Hello!", Encoding.UTF8);
            
            using var content = ContentLoader
                .For<LoadRequiredStringContent>()
                .WithDeserializer(s => new StreamReader(s, Encoding.UTF8).ReadToEnd())
                .LoadFromDirectory(tempDirectory);
            
            Assert.Equal("Hello!", content.StringProperty);
        }

        [Fact]
        public void LoadRequiredString_FileNotExists_Throws()
        {
            var content = ContentLoader
                .For<LoadRequiredStringContent>()
                .WithDeserializer(s => new StreamReader(s, Encoding.UTF8).ReadToEnd());
            
            Assert.Throws<FileNotFoundException>(() => content.LoadFromDirectory(tempDirectory));
        }

        private class InjectPathContent : TestContent
        {
            [FileContent("file.txt", InjectPath = true)]
            public string FilePath { get; private set; }
        }

        [Fact]
        public void InjectPath()
        {
            var filePath = Path.GetFullPath(Path.Combine(tempDirectory, "file.txt"));
            File.WriteAllText(filePath, "Hello!", Encoding.UTF8);
            
            using var content = ContentLoader
                .For<InjectPathContent>()
                .WithDeserializer(s => new StreamReader(s, Encoding.UTF8).ReadToEnd())
                .LoadFromDirectory(tempDirectory);
            
            Assert.Equal(filePath, content.FilePath);
        }
        
        private class LoadOptionalStringContent : TestContent
        {
            [FileContent("file.txt", Optional = true)]
            public string StringProperty { get; private set; }
        }
        
        [Fact]
        public void LoadOptionalString_FileExists_Success()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "file.txt"), "Hello!", Encoding.UTF8);
            
            using var content = ContentLoader
                .For<LoadOptionalStringContent>()
                .WithDeserializer(s => new StreamReader(s, Encoding.UTF8).ReadToEnd())
                .LoadFromDirectory(tempDirectory);
            
            Assert.Equal("Hello!", content.StringProperty);
        }
        
        [Fact]
        public void LoadOptionalString_FileNotExists_Success()
        {
            using var content = ContentLoader
                .For<LoadOptionalStringContent>()
                .WithDeserializer(s => new StreamReader(s, Encoding.UTF8).ReadToEnd())
                .LoadFromDirectory(tempDirectory);
            
            Assert.Null(content.StringProperty);
        }
        
        private class LoadGlobalStringContent : TestContent
        {
            [FileContent("file.txt", Global = true)]
            public string StringProperty { get; private set; }
        }

        [Fact] 
        public void LoadGlobalString_FileInCurrentDirectory_Success()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "file.txt"), "Hello!", Encoding.UTF8);
            
            using var content = ContentLoader
                .For<LoadGlobalStringContent>()
                .WithDeserializer(s => new StreamReader(s, Encoding.UTF8).ReadToEnd())
                .LoadFromDirectory(tempDirectory);
            
            Assert.Equal("Hello!", content.StringProperty);
        }
        
        [Fact] 
        public void LoadGlobalString_FileInParentDirectory_Success()
        {
            var parentDirectory = tempDirectory;
            var contentDirectory = Path.Combine(parentDirectory, "content_directory");
            Directory.CreateDirectory(contentDirectory);

            File.WriteAllText(Path.Combine(parentDirectory, "file.txt"), "Hello!", Encoding.UTF8);
            
            using var content = ContentLoader
                .For<LoadGlobalStringContent>()
                .WithDeserializer(s => new StreamReader(s, Encoding.UTF8).ReadToEnd())
                .LoadFromDirectory(contentDirectory);
            
            Assert.Equal("Hello!", content.StringProperty);
        }
        
        [Fact] 
        public void LoadGlobalString_FileInAncestor20Directory_Success()
        {
            var ancestorDirectory = tempDirectory;
            var currentContentDirectory = ancestorDirectory;
            for (var i = 0; i < 20; i++)
            {
                currentContentDirectory = Path.Combine(currentContentDirectory, $"dir_{i}");
                Directory.CreateDirectory(currentContentDirectory);
            }

            
            File.WriteAllText(Path.Combine(tempDirectory, "file.txt"), "Hello!", Encoding.UTF8);
            
            using var content = ContentLoader
                .For<LoadGlobalStringContent>()
                .WithDeserializer(s => new StreamReader(s, Encoding.UTF8).ReadToEnd())
                .LoadFromDirectory(currentContentDirectory);
            
            Assert.Equal("Hello!", content.StringProperty);
        }
        
        private class DeserializerNotExistsContent : TestContent
        {
            [FileContent("file.xml")]
            public XDocument Xml { get; private set; }
        }
        
        [Fact] 
        public void DeserializerNotExists_ThrowsAndFreeResources()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "file.xml"), "<xml />", Encoding.UTF8);

            Assert.Throws<InvalidOperationException>(() => ContentLoader
                .For<DeserializerNotExistsContent>()
                .LoadFromDirectory(tempDirectory));
        }

        private class FileNotFoundContent : TestContent
        {
            [FileContent("a.txt")]
            public string A { get; private set; }
            
            [FileContent("b.txt")]
            public string B { get; private set; }
            
            [FileContent("c.txt")]
            public string C { get; private set; }
        }
        
        [Fact] 
        public void FileNotFound_ThrowsAndFreeResources()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "a.txt"), "AAA", Encoding.UTF8);
            File.WriteAllText(Path.Combine(tempDirectory, "c.txt"), "CCC", Encoding.UTF8);

            Assert.Throws<FileNotFoundException>(() => ContentLoader
                .For<FileNotFoundContent>()
                .WithDeserializer(s => new StreamReader(s, Encoding.UTF8).ReadToEnd())
                .LoadFromDirectory(tempDirectory));
        }

        private class DifferentDeserializersContent : TestContent
        {
            [FileContent("file.xml")]
            public XDocument Xml { get; private set; }
            
            [FileContent("array.txt")]
            public int[] Array { get; private set; }
            
            [FileContent("bytes.bin")]
            public byte[] Bytes { get; private set; }
        }
        
        [Fact] 
        public void DifferentDeserializers_Success()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "file.xml"), "<xml />", Encoding.UTF8);
            File.WriteAllText(Path.Combine(tempDirectory, "array.txt"), "1,2,3", Encoding.UTF8);
            File.WriteAllBytes(Path.Combine(tempDirectory, "bytes.bin"), new byte[] {1, 2, 3});

            using var content = ContentLoader
                .For<DifferentDeserializersContent>()
                .WithDeserializer(XDocument.Load)
                .WithDeserializer(s => new StreamReader(s).ReadToEnd().Split(",").Select(int.Parse).ToArray())
                .WithDeserializer(s =>
                {
                    var ms = new MemoryStream();
                    s.CopyTo(ms);
                    return ms.ToArray();
                })
                .LoadFromDirectory(tempDirectory);

            Assert.Equal("xml", content.Xml.Root.Name.LocalName);
            Assert.Equal(new[] {1, 2, 3}, content.Array);
            Assert.Equal(new byte[] {1, 2, 3}, content.Bytes);
        }

        private class MultipleFilesContent : TestContent
        {
            [FileContent("*file.txt", Multiple = true)]
            public IReadOnlyCollection<string> Files { get; private set; }
        }

        [Fact]
        public void MultipleFiles()
        {
            File.WriteAllText(Path.Combine(tempDirectory, "1file.txt"), "1", Encoding.UTF8);
            File.WriteAllText(Path.Combine(tempDirectory, "2file.txt"), "2", Encoding.UTF8);
            File.WriteAllText(Path.Combine(tempDirectory, "3file.txt"), "3", Encoding.UTF8);

            using var content = ContentLoader
                .For<MultipleFilesContent>()
                .WithDeserializer(s => s.ReadString(Encoding.UTF8))
                .LoadFromDirectory(tempDirectory);
            
            Assert.Equal(new[] {"1", "2", "3"}, content.Files.OrderBy(x => x).ToList());
        }

        private class InjectMultiplePathsContent : TestContent
        {
            [FileContent("*file.txt", Multiple = true, InjectPath = true)]
            public IReadOnlyCollection<string> Files { get; private set; }
        }

        [Fact]
        public void InjectMultiplePaths()
        {
            var paths = new[]
                {
                    "1file.txt",
                    "2file.txt",
                    "3file.txt"
                }
                .Select(fileName => Path.GetFullPath(Path.Combine(tempDirectory, fileName)))
                .ToList();

            foreach (var path in paths)
            {
                File.WriteAllText(path, "x", Encoding.UTF8);
            }

            using var content = ContentLoader
                .For<InjectMultiplePathsContent>()
                .LoadFromDirectory(tempDirectory);

            Assert.Equal(paths, content.Files.OrderBy(x => x).ToList());
        }
        
        private class MultipleRequiredFileContent : TestContent
        {
            [FileContent("*file.txt", Multiple = true)]
            public IReadOnlyCollection<string> Files { get; private set; }
        }
        
        [Fact]
        public void MultipleRequiredFile_Throws()
        {
            Assert.Throws<FileNotFoundException>(
                () => ContentLoader
                    .For<MultipleRequiredFileContent>()
                    .LoadFromDirectory(tempDirectory));
        }
        
        private class MultipleOptionalFilesWithInitialValue : TestContent
        {
            [FileContent("*file.txt", Multiple = true, Optional = true)]
            public IReadOnlyCollection<string> Files { get; private set; } = new List<string>() {"abc"};
        }
        
        [Fact]
        public void MultipleOptionalFiles_DoesNotOverwriteInitialValue()
        {
            using var content = ContentLoader
                .For<MultipleOptionalFilesWithInitialValue>()
                .LoadFromDirectory(tempDirectory);
            
            Assert.Equal(new []{"abc"}, content.Files);
        }
        
        private class MultipleOptionalFiles : TestContent
        {
            [FileContent("*file.txt", Multiple = true, Optional = true)]
            public IReadOnlyCollection<string> Files { get; private set; }
        }
        
        [Fact]
        public void MultipleOptionalFile_CreateEmptyList()
        {
            using var content = ContentLoader
                .For<MultipleOptionalFiles>()
                .LoadFromDirectory(tempDirectory);
            
            Assert.NotNull(content.Files);
            Assert.Empty(content.Files);
        }
        
        private class OptionalFileWithInitialValue : TestContent
        {
            [FileContent("*file.txt", Optional = true)]
            public string File { get; private set; } = "abc";
        }
        
        [Fact]
        public void OptionalFile_DoesNotOverwriteInitialValue()
        {
            using var content = ContentLoader
                .For<OptionalFileWithInitialValue>()
                .LoadFromDirectory(tempDirectory);
            
            Assert.Equal("abc", content.File);
        }

        private class MultipleFileInvalidTypeContent : TestContent
        {
            [FileContent("*file.txt", Optional = true, Multiple = true)]
            public List<string> File { get; private set; }
        }
        
        [Fact]
        public void MultipleFile_PropertyHasInvalidType_Throws()
        {
            Assert.Throws<InvalidOperationException>(
                () => ContentLoader
                    .For<MultipleFileInvalidTypeContent>()
                    .LoadFromDirectory(tempDirectory));
        }
    }
}