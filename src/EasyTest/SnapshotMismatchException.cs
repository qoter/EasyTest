using System;
using System.IO;

namespace EasyTest
{
    public class SnapshotMismatchException : Exception
    {
        public override string Message => message;
        private readonly string message;

        public SnapshotMismatchException(
            string expected,
            string actual,
            int diffPosition,
            string mismatchPath,
            string snapshotPath)
        {
            message = CreateMessage(expected, actual, diffPosition, mismatchPath, snapshotPath);
        }

        private static string CreateMessage(
            string expected,
            string actual,
            int diffPosition,
            string mismatchPath,
            string snapshotPath)
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
{ClickableCommands.CreateAcceptAllDiffsCommand(Path.GetDirectoryName(mismatchPath), Path.GetDirectoryName(snapshotPath))}
";
        }
    }
}