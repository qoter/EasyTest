using System.IO;
using System.Linq;

namespace EasyTest
{
    internal static class FileFinder
    {
        public static string FindPathToFileGlobal(string directory, string fileTemplate)
        {
            var directoryInfo = new DirectoryInfo(directory);
            while (directoryInfo != null)
            {
                var path = FindPathToFile(directoryInfo.FullName, fileTemplate);
                if (path != null)
                    return path;
                directoryInfo = directoryInfo.Parent;
            }

            return null;
        }

        public static string FindPathToFile(string directory, string fileTemplate)
        {
            return fileTemplate
                .Split('|')
                .SelectMany(searchPattern => Directory.EnumerateFiles(directory, searchPattern))
                .FirstOrDefault();
        }
    }
}