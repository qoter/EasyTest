using System;
using System.IO;

namespace EasyTest
{
    public class ContentVerifierWithActual
    {
        private readonly string directory;
        private readonly string actualFileName;
        private readonly Action<Stream> writeActual;

        internal ContentVerifierWithActual(string directory, string actualFileName, Action<Stream> writeActual)
        {
            this.directory = directory ?? throw new ArgumentNullException(nameof(directory));
            this.actualFileName = actualFileName ?? throw new ArgumentNullException(nameof(actualFileName));
            this.writeActual = writeActual ?? throw new ArgumentNullException(nameof(writeActual));
        }
        
        public ContentVerifierWithExpected<TExpected> ReadExpectedAs<TExpected>(string expectedFileName, Func<Stream, TExpected> readExpected)
        {
            if (expectedFileName == null) throw new ArgumentNullException(nameof(expectedFileName));
            if (readExpected == null) throw new ArgumentNullException(nameof(readExpected));
            
            return new ContentVerifierWithExpected<TExpected>(directory, actualFileName, writeActual, expectedFileName, readExpected);
        }
    }
}