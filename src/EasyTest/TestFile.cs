using System;

namespace EasyTest
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TestFile : Attribute
    {
        public readonly string FileTemplate;

        public bool Optional { get; set; } = false;

        public bool Global { get; set; } = false;

        public bool InjectPath { get; set; } = false;

        public TestFile(string fileTemplate)
        {
            FileTemplate = fileTemplate;
        }
    }
}