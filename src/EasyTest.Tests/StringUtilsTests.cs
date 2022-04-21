using System;
using Xunit;

namespace EasyTest.Tests
{
    public class StringUtilsTests
    {
        [Theory]
        [InlineData("", "", -1)]
        [InlineData("a", "a", -1)]
        [InlineData("abc", "abc", -1)]
        [InlineData("", "abc", 0)]
        [InlineData("aaa", "baaa", 0)]
        [InlineData("aaa", "aaab", 3)]
        [InlineData("aaa", "aab", 2)]
        [InlineData("aba", "aaa", 1)]
        [InlineData("aaaaaaaaaaa", "aaaaabaaaaaaaaaa", 5)]
        public void FindDiffIndexTests(string a, string b, int expectedDiffIndex)
        {
            Assert.Equal(expectedDiffIndex, StringUtils.FindDiffIndex(a, b));
            Assert.Equal(expectedDiffIndex, StringUtils.FindDiffIndex(b, a));
        }

        [Theory]
        [InlineData("", -1)]
        [InlineData("", 0)]
        [InlineData("", 1)]
        [InlineData("a", 1)]
        [InlineData("abc", -1)]
        [InlineData("abc", 3)]
        public void GetViewAroundIndex_ThrowsIndexOutOfRange(string line, int index)
        {
            Assert.Throws<IndexOutOfRangeException>(() => StringUtils.GetViewAroundIndex(line, index, 10));
        }
        
        [Theory]
        [InlineData("abc", 1, 0, "...b...", 3)]
        [InlineData("abaaaa", 1, 1, "aba...", 1)]
        [InlineData("aaaaba", 4, 1, "...aba", 4)]
        [InlineData("a", 0, 5, "a", 0)]
        [InlineData("aaaabaaaa", 4, 2, "...aabaa...", 5)]
        [InlineData("a\n\n\n\r\t\0ab", 4, 3, "...\\n\\n\\n\\r\\t\\0a...", 9)]
        public void GetViewAroundIndexTests(string line, int index, int size, string expectedView, int expectedIndex)
        {
            var (actualView, actualIndex) = StringUtils.GetViewAroundIndex(line, index, size);
            
            Assert.Equal(expectedView, actualView);
            Assert.Equal(expectedIndex, actualIndex);
        }
    }
}