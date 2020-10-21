using System;
using System.IO;
using System.Linq;
using EasyTest.Utils;
using Xunit;

namespace EasyTest.Tests
{
    public class UsageExample : IDisposable
    {
        private readonly string testDirectory;

        #region prepare test directory and remove it at end of testing

        public UsageExample()
        {
            testDirectory = Guid.NewGuid().ToString("N");
            Directory.CreateDirectory(testDirectory);
        }

        public void Dispose()
        {
            if (testDirectory != null && Directory.Exists(testDirectory))
                Directory.Delete(testDirectory, true);
        }

        #endregion

        public class MyContent : TestContent
        {
            [FileContent("numbers.num")] public int[] Numbers { get; private set; }

            [FileContent("rules.rul")] public string[] Rules { get; private set; }
        }

        [Fact]
        public void TestApplyRulesToNumbers()
        {
            PrepareDirectory();

            // arrange
            using var content = ContentLoader
                .For<MyContent>()
                .WithDeserializer(s => s.ReadString().Split(",").Select(int.Parse).ToArray()) // for numbers
                .WithDeserializer(s => s.ReadString().Split("\n")) // for rules
                .LoadFromDirectory(testDirectory);

            // act
            var actualNumbers = ApplyRulesToNumbers(content.Numbers, content.Rules);

            // assert
            ContentVerifier
                .UseDirectory(testDirectory)
                .SaveActualAs("actual.num", s => s.WriteString(string.Join(",", actualNumbers)))
                .ReadExpectedAs("expected.num", s => s.ReadString().Split(",").Select(int.Parse).ToArray())
                .Verify(expectedNumbers => Assert.Equal(expectedNumbers, actualNumbers));
        }
        
        private void PrepareDirectory()
        {
            File.WriteAllText(Path.Combine(testDirectory, "numbers.num"), "0,1,2,3,4,5,6");
            File.WriteAllText(Path.Combine(testDirectory, "rules.rul"), @"
remove odd  # 0,2,4,6
reverse all # 6,4,2,0
shift 2     # 2,0,6,4
");
            File.WriteAllText(Path.Combine(testDirectory, "expected.num"), "2,0,6,4");
        }


        private static int[] ApplyRulesToNumbers(int[] numbers, string[] rules)
        {
            var current = numbers.ToArray();
            foreach (var rule in rules)
            {
                var ruleTokens = rule.Split(" ").ToList();
                if (ruleTokens.Count < 2)
                    continue;

                var command = ruleTokens[0];
                var param = ruleTokens[1];

                switch (command)
                {
                    case "reverse":
                        Array.Reverse(current);
                        break;

                    case "remove":
                        var rest = param == "even" ? 0 : 1;
                        current = current.Where(x => x % 2 != rest).ToArray();
                        break;

                    case "shift":
                        int.TryParse(param, out var k);
                        if (k == 0 || k >= current.Length)
                            continue;

                        Array.Reverse(current, 0, k);
                        Array.Reverse(current, k, current.Length - k);
                        Array.Reverse(current);
                        break;
                }
            }

            return current;
        }
    }
}