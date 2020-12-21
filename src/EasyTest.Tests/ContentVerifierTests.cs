using System;
using System.IO;
using EasyTest.Utils;
using Xunit;
using Xunit.Sdk;

namespace EasyTest.Tests
{
    public class ContentVerifierTests : IDisposable
    {
        private readonly string tempDirectory;

        public ContentVerifierTests()
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
        public void VerifyWithoutExpected_ThrowExpectedFileNotFoundAndSaveActual()
        {
            const string actualFileName = "actual.txt";
            const string expectedFileName = "expected.txt";
            const string actualFileContent = "actual file content";
            var actualFilePath = Path.Combine(tempDirectory, actualFileName);

            void VerifyWithoutExpected() =>
                ContentVerifier.UseDirectory(tempDirectory)
                    .SaveActualAs(actualFileName, s => WriteStringToStream(s, actualFileContent))
                    .ReadExpectedAs(expectedFileName, s => new StreamReader(s).ReadToEnd())
                    .Verify(expected => Assert.True(false));

            Assert.Throws<ExpectedFileNotFoundException>(VerifyWithoutExpected);
            Assert.True(File.Exists(actualFilePath), $"File.Exists(\"{actualFilePath}\") return false");
            Assert.Equal(File.ReadAllText(actualFilePath), actualFileContent);
        }

        [Fact]
        public void VerifyWithBadActual_ThrowAndSaveActual()
        {
            const string actualFileName = "actual.txt";
            const string expectedFileName = "expected.txt";
            const string actualContent = "actual file content";
            const string expectedContent = "expected content";
            var actualFilePath = Path.Combine(tempDirectory, actualFileName);
            var expectedFilePath = Path.Combine(tempDirectory, expectedFileName);
            File.WriteAllText(expectedFilePath, expectedContent);

            void VerifyWithBadActual() =>
                ContentVerifier.UseDirectory(tempDirectory)
                    .SaveActualAs(actualFileName, s => WriteStringToStream(s, actualContent))
                    .ReadExpectedAs(expectedFileName, s => new StreamReader(s).ReadToEnd())
                    .Verify(expected => Assert.Equal(actualContent, expected));

            Assert.Throws<EqualException>(VerifyWithBadActual);
            Assert.True(File.Exists(actualFilePath), $"File.Exists(\"{actualFilePath}\") return false");
            Assert.Equal(File.ReadAllText(actualFilePath), actualContent);
        }

        [Fact]
        public void VerifyWithGoodActual_DoNothing()
        {
            const string actualFileName = "actual.txt";
            const string expectedFileName = "expected.txt";
            const string actualContent = "good content";
            const string expectedContent = "good content";
            var actualFilePath = Path.Combine(tempDirectory, actualFileName);
            var expectedFilePath = Path.Combine(tempDirectory, expectedFileName);
            File.WriteAllText(expectedFilePath, expectedContent);

            ContentVerifier.UseDirectory(tempDirectory)
                .SaveActualAs(actualFileName, s => WriteStringToStream(s, actualContent))
                .ReadExpectedAs(expectedFileName, s => new StreamReader(s).ReadToEnd())
                .Verify(expected => Assert.Equal(actualContent, expected));
            
            Assert.False(File.Exists(actualFilePath), $"File.Exists(\"{actualFilePath}\") return true");
        }

        [Fact]
        public void ActualFileOverwriteOldActual()
        {
            const string actualFileName = "actual.txt";
            const string expectedFileName = "expected.txt";
            const string actualContent = "old looooooooooooooooooooooooooooong actual";
            const string expectedContent = "good content";
            var actualFilePath = Path.Combine(tempDirectory, actualFileName);
            var expectedFilePath = Path.Combine(tempDirectory, expectedFileName);
            File.WriteAllText(expectedFilePath, expectedContent);
            File.WriteAllText(actualFilePath, actualContent);

            void Verify() =>
                ContentVerifier.UseDirectory(tempDirectory)
                    .SaveActualAs(actualFileName, s => s.WriteString("new short actual"))
                    .ReadExpectedAs(expectedFileName, s => s.ReadString())
                    .Verify(expected => Assert.False(true));

            Assert.Throws<FalseException>(Verify);
            Assert.Equal("new short actual", File.ReadAllText(actualFilePath));
        }

        private static void WriteStringToStream(Stream s, string str)
        {
            using var writer = new StreamWriter(s);
            writer.Write(str);
        }
    }
}