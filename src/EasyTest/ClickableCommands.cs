namespace EasyTest
{
    internal static class ClickableCommands
    {
        public static string CreateViewDiffCommand(string leftPath, string rightPath)
        {
            return $"<view diff> $(rider) diff \"{leftPath}\" \"{rightPath}\"";
        }

        public static string CreateNavigateCommand(string path)
        {
            return $"<view in project> $(rider) navigate \"{path}\"";
        }
        
        public static string CreateAcceptDiffCommand(string actualPath, string expectedPath)
        {
            return $"<accept diff> $(term) move /Y \"{actualPath}\" \"{expectedPath}\"";
        }

        public static string CreateAcceptAllDiffsCommand(string mismatchDirectory, string snapshotDirectory)
        {
            return $"<accept ALL diffs> $(term) move /Y \"{mismatchDirectory}\\*\" \"{snapshotDirectory}\" & rmdir /S /Q \"{mismatchDirectory}\"";
        }
    }
}