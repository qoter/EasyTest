using System;
using Xunit;

namespace EasyTest.Tests
{
    public class ExceptionMessageTests
    {
        [Fact]
        public void ContentVerificationExceptionMessage()
        {
            var exception = new ContentVerificationException(
                new Exception("inner exception message"),
                "/path/to/expected.txt",
                "/path/to/actual.txt",
                "/path/to/test/diretory");
            
            Snapshoter.MatchSnapshot(exception.Message);
        }

        [Fact]
        public void ExpectedFileNotFoundExceptionMessage()
        {
            var exception = new ExpectedFileNotFoundException(
                "/path/to/expected.txt",
                "/path/to/actual.txt",
                "/path/to/test/directory");
            
            Snapshoter.MatchSnapshot(exception.Message);
        }

        [Fact]
        public void SnapshotMismatchExceptionMessage()
        {
            var exception = new SnapshotMismatchException(
                "some expected text\n\r\t\0", 
                "some actual text\n\r\t\0", 
                3, 
                "/path/to/mismatch.snap",
                "/path/to/snapshot.snap",
                "/path/to/mismatch",
                "/path/to/snapshots");
            
            Snapshoter.MatchSnapshot(exception.Message);
        }
    }
}