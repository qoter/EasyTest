using System;
using System.IO;
using System.Linq;
using EasyTest.Utils;
using Xunit;

namespace EasyTest.Tests
{
    public class UsageExample
    {
        public class MyContent : TestContent
        {
            [FileContent("numbers.num")] public int[] Numbers { get; private set; }

            [FileContent("rules.rul")] public string[] Rules { get; private set; }
        }

        [Fact]
        public void TestApplyRulesToNumbers()
        {
            var testDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "TestData", "example");

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