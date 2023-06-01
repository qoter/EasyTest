using System;
using System.Collections.Generic;
using System.Text;

namespace EasyTest
{
    internal static class StringUtils
    {
        private static readonly IReadOnlyDictionary<char, string> EscapedSymbols = new Dictionary<char, string>
        {
            {'\n', "\\n"},
            {'\r', "\\r"},
            {'\t', "\\t"},
            {'\0', "\\0"},
        };
        
        public static int FindDiffIndex(string left, string right)
        {
            for (var i = 0; i < Math.Min(left.Length, right.Length); i++)
            {
                if (left[i] != right[i])
                    return i;
            }

            if (left.Length == right.Length)
                return -1;

            return Math.Min(left.Length, right.Length);
        }
        
        public static string SystemifyLineEnding(string s)
        {
            return s.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
        }
        
        public static (string stringView, int indexView) GetViewAroundIndex(string line, int index, int radius)
        {
            if (radius < 0)
                throw new ArgumentException($"{nameof(radius)} should be greater or equal 0");
            if (index < 0 || index > line.Length)
                throw new IndexOutOfRangeException();
            
            var start = Math.Max(0, index - radius);
            var end = Math.Min(line.Length, index + radius + 1);

            var result = new StringBuilder();
            if (start > 0)
                result.Append("...");

            var newIndex = -1;
            for (var i = start; i < end; i++)
            {
                if (i == index)
                    newIndex = result.Length;
                
                var ch = line[i];
                if (EscapedSymbols.TryGetValue(ch, out var escapedChar))
                {
                    result.Append(escapedChar);
                }
                else
                {
                    result.Append(ch);
                }
            }
            
            if (end < line.Length)
                result.Append("...");
            
            if (newIndex == -1)
                newIndex = result.Length;

            return (result.ToString(), newIndex);
        }
    }
}