using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace EasyTest
{
    public static class Snapshoter
    {
        private const string SnapshotDirectoryName = "__snapshots__";
        private const string MismatchDirectoryName = "__mismatch__";
        
        public static void Match(string actualValue)
        {
            var commonSnapshotDirectory = FindSnapshotDirectory();
            var callerFrame = (new StackTrace().GetFrames() ?? Array.Empty<StackFrame>()).Skip(1).FirstOrDefault();
            var (className, methodName) = GetClassAndMethodNames(callerFrame);

            var snapshotFileName = $"{methodName}.snap";

            var mismatchDirectory = Path.Combine(commonSnapshotDirectory, className, MismatchDirectoryName);
            var snapshotDirectory = Path.Combine(commonSnapshotDirectory, className);
            
            Directory.CreateDirectory(snapshotDirectory);

            var snapshotFilePath = Path.Combine(snapshotDirectory, snapshotFileName);
            var mismatchFilePath = Path.Combine(mismatchDirectory, snapshotFileName);

            CreateEmptyFileIfNotExists(snapshotFilePath);

            var expectedValue = File.ReadAllText(snapshotFilePath);

            actualValue = SystemifyLineEnding(actualValue);
            expectedValue = SystemifyLineEnding(expectedValue);

            if (actualValue == expectedValue)
            {
                DeleteFileIfExists(mismatchFilePath);
                DeleteDirectoryIfEmpty(mismatchDirectory);
            }
            else
            {
                Directory.CreateDirectory(mismatchDirectory);

                File.WriteAllText(mismatchFilePath, actualValue);

                Console.WriteLine(ClickableCommands.CreateViewDiffCommand(mismatchFilePath, snapshotFilePath));
                Console.WriteLine(ClickableCommands.CreateAcceptDiffCommand(mismatchFilePath, snapshotFilePath));
                Console.WriteLine(ClickableCommands.CreateAcceptAllDiffsCommand(mismatchDirectory, snapshotDirectory));
                
                throw new SnapshotMismatchException();
            }

        }

        private static (string className, string methodName) GetClassAndMethodNames(StackFrame callerFrame)
        {
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

            return (className, methodName);
        }

        private static void CreateEmptyFileIfNotExists(string snapshotFilePath)
        {
            if (!File.Exists(snapshotFilePath))
            {
                File.WriteAllText(snapshotFilePath, "");
            }
        }

        private static void DeleteFileIfExists(string mismatchFilePath)
        {
            if (File.Exists(mismatchFilePath))
            {
                File.Delete(mismatchFilePath);
            }
        }

        private static void DeleteDirectoryIfEmpty(string mismatchDirectory)
        {
            if (Directory.Exists(mismatchDirectory) && !Directory.EnumerateFiles(mismatchDirectory).Any())
            {
                Directory.Delete(mismatchDirectory);
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