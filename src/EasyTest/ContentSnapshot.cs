using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EasyTest
{
    public static class ContentSnapshot
    {
        private const string SnapshotDirectoryName = "__snapshot__";
        private const string MismatchDirectoryName = "__mismatch__";
        
        public static void Verify(string actualValue)
        {
            var commonSnapshotDirectory = FindSnapshotDirectory();
            var callerFrame = (new StackTrace().GetFrames() ?? Array.Empty<StackFrame>()).Skip(1).FirstOrDefault();
            if (callerFrame == null)
            {
                throw new InvalidOperationException("Caller not found");
            }
            
            var methodName = callerFrame.GetMethod().Name;
            
            var className = callerFrame.GetMethod().DeclaringType?.Name;
            if (className == null)
            {
                throw new InvalidOperationException($"DeclaringType of method {methodName} not found");
            }

            var snapshotFileName = $"{methodName}.snap";

            var mismatchDirectory = Path.Combine(commonSnapshotDirectory, className, MismatchDirectoryName);
            var snapshotDirectory = Path.Combine(commonSnapshotDirectory, className);

            if (!Directory.Exists(snapshotDirectory))
            {
                Directory.CreateDirectory(snapshotDirectory);
            }

            var snapshotFilePath = Path.Combine(snapshotDirectory, snapshotFileName);
            var mismatchFilePath = Path.Combine(mismatchDirectory, snapshotFileName);

            if (!File.Exists(snapshotFilePath))
            {
                File.WriteAllText(snapshotFilePath, "");
            }

            var expectedValue = File.ReadAllText(snapshotFilePath);

            actualValue = SystemifyLineEnding(actualValue);
            expectedValue = SystemifyLineEnding(expectedValue);

            if (actualValue == expectedValue)
            {
                if (File.Exists(mismatchFilePath))
                {
                    File.Delete(mismatchFilePath);
                }

                if (Directory.Exists(mismatchDirectory) && !Directory.EnumerateFiles(mismatchDirectory).Any())
                {
                    Directory.Delete(mismatchDirectory);
                }
            }
            else
            {
                if (!Directory.Exists(mismatchDirectory))
                {
                    Directory.CreateDirectory(mismatchDirectory);
                }
                
                File.WriteAllText(mismatchFilePath, actualValue);

                Console.WriteLine($"<view diff> $(rider) diff \"{mismatchFilePath}\" \"{snapshotFilePath}\"");
                Console.WriteLine($"<accept diff> $(term) move /Y \"{mismatchFilePath}\" \"{snapshotFilePath}\"");
                Console.WriteLine($"<accept ALL diffs> $(term) move /Y \"{mismatchDirectory}\\*\" \"{snapshotDirectory}\" & rmdir /S /Q \"{mismatchDirectory}\"");

                throw new SnapshotMismatchException();
            }
        }

        private static string FindSnapshotDirectory()
        {
            var currentDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (currentDirectory != null)
            {           
                var possiblePath = Path.Combine(currentDirectory.FullName, SnapshotDirectoryName);
                if (Directory.Exists(possiblePath))
                    return possiblePath;

                currentDirectory = currentDirectory.Parent;
            }
            
            throw new InvalidExpressionException($"Can't find {SnapshotDirectoryName} directory. " +
                                                 "We search it in all directories which top of " +
                                                 "AppDomain.CurrentDomain.BaseDirectory.");
        }

        private static string SystemifyLineEnding(string s)
        {
            return s.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
        }
    }
}