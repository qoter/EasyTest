using System.Xml.Linq;
using Xunit;
using Xunit.Abstractions;

namespace EasyTest.Tests
{
    public class SnapshoterUsageExample
    {
        private readonly ITestOutputHelper output;

        public SnapshoterUsageExample(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void CheckSomeGeneratedXml_Ok()
        {
            var xml = GenerateXml(20);
            
            Snapshoter.MatchSnapshot(xml.ToString());
        }
        
        [Fact]
        public void CheckSomeGeneratedXml_Fail()
        {
            var xml = GenerateXml(20);

            var wasException = false;
            try
            {
                Snapshoter.MatchSnapshot(xml.ToString());
            }
            catch (SnapshotMismatchException e)
            {
                output.WriteLine("This is an example of failed test");
                output.WriteLine(e.ToString());
                wasException = true;
            }
            
            Assert.True(wasException);
        }

        private static XElement GenerateXml(int count)
        {
            var root = new XElement("Root");
            for (var i = 0; i < count; i++)
            {
                root.Add(new XElement($"Element{i}"));
            }

            return root;
        }
    }
}