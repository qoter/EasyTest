using System;

namespace EasyTest
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FileContent : Attribute
    {
        /// <summary>
        /// File template for search file.
        /// Can contain a combination of valid literal path, pipe (|) and wildcard (* and ?) characters.
        /// Pipe can separate different alternatives of files (template like 'fileA|fileB' means search 'fileA' or 'fileB')
        /// </summary>
        public string FileTemplate { get; }

        /// <summary>
        /// Mark content optional, does not throws if file does not exists.
        /// </summary>
        public bool Optional { get; set; } = false;

        /// <summary>
        /// Search file in top directories.
        /// </summary>
        public bool Global { get; set; } = false;

        /// <summary>
        /// Doesn't deserialize file, write only path to property.
        /// </summary>
        public bool InjectPath { get; set; } = false;
        
        public FileContent(string fileTemplate)
        {
            FileTemplate = fileTemplate;
        }
    }
}