using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EasyTest
{
    internal static class FileFinder
    {
        public static IEnumerable<string> FindPaths(string directory, string fileTemplate, bool global = false)
        {
            return global 
                ? FindPathsGlobal(directory, fileTemplate) 
                : FindPathsInDirectory(directory, fileTemplate);
        }
        
        private static IEnumerable<string> FindPathsGlobal(string directory, string fileTemplate)
        {
            var directoryInfo = new DirectoryInfo(directory);
            while (directoryInfo != null)
            {
                foreach (var path in FindPathsInDirectory(directoryInfo.FullName, fileTemplate))
                {
                    yield return path;
                }

                directoryInfo = directoryInfo.Parent;
            }
        }

        private static IEnumerable<string> FindPathsInDirectory(string directory, string fileTemplate)
        {
            
            return fileTemplate
                .Split('|')
                .SelectMany(searchPattern => Directory.EnumerateFiles(directory, searchPattern))
                .Select(Path.GetFullPath);
        }
    }
}