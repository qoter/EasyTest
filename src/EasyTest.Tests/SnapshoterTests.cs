using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace EasyTest.Tests
{
    public class SnapshoterTests : IDisposable
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly string snapshotsDirectory;

        public SnapshoterTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            snapshotsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, "__snapshots__");
            Directory.CreateDirectory(snapshotsDirectory);
        }

        public void Dispose()
        {
            if (snapshotsDirectory != null && Directory.Exists(snapshotsDirectory))
                Directory.Delete(snapshotsDirectory, true);
        }

        [Fact]
        public void MatchFirstTime_SaveMismatchAndEmptySnapshot()
        {
            var mismatchFilePath = Path.Combine(
                snapshotsDirectory, 
                nameof(SnapshoterTests), 
                "__mismatch__", 
                $"{nameof(MatchFirstTime_SaveMismatchAndEmptySnapshot)}.snap");
            var snapshotFilePath = Path.Combine(
                snapshotsDirectory, 
                nameof(SnapshoterTests),
                $"{nameof(MatchFirstTime_SaveMismatchAndEmptySnapshot)}.snap");
            
            void Match() => Snapshoter.MatchSnapshot("abc");
            
            Assert.Throws<SnapshotMismatchException>(Match);
            Assert.True(File.Exists(mismatchFilePath));
            Assert.True(File.Exists(snapshotFilePath));
            Assert.Equal("abc", File.ReadAllText(mismatchFilePath));
            Assert.Empty(File.ReadAllText(snapshotFilePath));
        }
        
        [Fact]
        public void MatchWithDifferenceLineEnding_DoesNotThrows()
        {
            var snapshotFilePath = Path.Combine(
                snapshotsDirectory, 
                nameof(SnapshoterTests),
                $"{nameof(MatchWithDifferenceLineEnding_DoesNotThrows)}.snap");
            Directory.CreateDirectory(Path.GetDirectoryName(snapshotFilePath));
            
            File.WriteAllText(snapshotFilePath, "abc\r\nbc");
        }

        [Fact]
        public void GenerateExceptionWithAwesomeDiffMessage()
        {
            var snapshotFilePath = Path.Combine(
                snapshotsDirectory, 
                nameof(SnapshoterTests),
                $"{nameof(GenerateExceptionWithAwesomeDiffMessage)}.snap");
            Directory.CreateDirectory(Path.GetDirectoryName(snapshotFilePath));
            File.WriteAllText(snapshotFilePath, new string('a', 200));

            var exception = false;
            try
            {
                Snapshoter.MatchSnapshot(new string('a', 100) + 'b' + new string('a', 99));
            }
            catch (SnapshotMismatchException e)
            {
                exception = true;
                Assert.StartsWith(@"Snapshot mismatch:
Expected: ...aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa...
Actual:   ...aaaaaaaaaaaaaaaaaaaaaaaaaaaaaabaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa...
                                           ↑ [100]

Available commands:", e.Message);
                _testOutputHelper.WriteLine(e.Message);
            }
            
            Assert.True(exception, "Should throws SnapshotMismatchException");
        }
    }
}