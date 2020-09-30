using System;

namespace EasyTest
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FileContent : Attribute
    {
        public readonly string FileTemplate;

        public bool Optional { get; set; } = false;

        public bool Global { get; set; } = false;

        public bool InjectPath { get; set; } = false;

        public FileContent(string fileTemplate)
        {
            FileTemplate = fileTemplate;
        }
    }
}