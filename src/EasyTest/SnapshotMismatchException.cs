using System;
using System.IO;

namespace EasyTest
{
    public class SnapshotMismatchException : Exception
    {
        public override string Message => message;
        private readonly string message;

        internal SnapshotMismatchException(
            string expected,
            string actual,
            int diffPosition,
            string mismatchPath,
            string snapshotPath,
            string mismatchDirectory,
            string snapshotsDirectory)
        {
            message = CreateMessage(
                expected,
                actual,
                diffPosition,
                mismatchPath,
                snapshotPath,
                mismatchDirectory,
                snapshotsDirectory);
        }

        private static string CreateMessage(
            string expected,
            string actual,
            int diffPosition,
            string mismatchPath,
            string snapshotPath,
            string mismatchDirectory,
            string snapshotsDirectory)
        {
            var (expectedView, _) = string.IsNullOrEmpty(expected) 
                ? ("", 0) 
                : StringUtils.GetViewAroundIndex(expected, diffPosition, 30);
            var (actualView, indexView) = string.IsNullOrEmpty(actual) 
                ? ("", 0) 
                : StringUtils.GetViewAroundIndex(actual, diffPosition, 30);
            
            var indexLine = new string(' ', indexView + "Expected: ".Length) + $"↑ [{diffPosition}]";
            
            return @$"Snapshot mismatch:
Expected: {expectedView}
Actual:   {actualView}
{indexLine}

Available commands:
{ClickableCommands.CreateViewDiffCommand(mismatchPath, snapshotPath)}
{ClickableCommands.CreateAcceptDiffCommand(mismatchPath, snapshotPath)}
{ClickableCommands.CreateAcceptAllDiffsCommand(mismatchDirectory, snapshotsDirectory)}
";
        }
    }
}