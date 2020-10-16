using System;
using System.IO;

namespace EasyTest
{
    public class ContentVerifier
    {
        private readonly string directory;

        private ContentVerifier(string directory)
        {
            this.directory = directory ?? throw new ArgumentNullException(nameof(directory));
        }
        public static ContentVerifier UseDirectory(string directory)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));

            return new ContentVerifier(directory);
        }

        public ContentVerifierWithActual SaveActualAs(string actualFileName, Action<Stream> writeActual)
        {
            if (actualFileName == null) throw new ArgumentNullException(nameof(actualFileName));
            if (writeActual == null) throw new ArgumentNullException(nameof(writeActual));
            
            return new ContentVerifierWithActual(directory, actualFileName, writeActual);
        }
    }
}