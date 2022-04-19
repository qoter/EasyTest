using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EasyTest
{
    public static class Snapshoter
    {
        private const string CommonSnapshotDirectoryName = "__snapshots__";
        private const string MismatchDirectoryName = "__mismatch__";
        
        public static void Match(
            string actualValue, 
            [CallerFilePath] string callerPath = "", 
            [CallerMemberName] string callerName = "")
        {
            if (string.IsNullOrEmpty(callerPath))
                throw new ArgumentException("Value cannot be null or empty.", nameof(callerPath));
            if (string.IsNullOrEmpty(callerName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(callerName));


            var commonSnapshotDirectory = FindCommonSnapshotDirectory();

            var snapshotFileName = $"{callerName}.snap";

            var snapshotDirectory = Path.Combine(commonSnapshotDirectory, Path.GetFileNameWithoutExtension(callerPath));
            var mismatchDirectory = Path.Combine(snapshotDirectory, MismatchDirectoryName);

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

        private static string FindCommonSnapshotDirectory()
        {
            var currentDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (currentDirectory != null)
            {           
                var possiblePath = Path.Combine(currentDirectory.FullName, CommonSnapshotDirectoryName);
                if (Directory.Exists(possiblePath))
                    return possiblePath;

                currentDirectory = currentDirectory.Parent;
            }
            
            throw new InvalidExpressionException($"Can't find {CommonSnapshotDirectoryName} directory. " +
                                                 "We search it in all directories which top of " +
                                                 "AppDomain.CurrentDomain.BaseDirectory.");
        }

        private static string SystemifyLineEnding(string s)
        {
            return s.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
        }
    }
}