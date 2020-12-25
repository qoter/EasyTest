using System.IO;
using System.Text;

namespace EasyTest.Utils
{
    public static class StreamExtensions
    {
        public static string ReadString(this Stream stream, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var streamReader = new StreamReader(stream, encoding, true);
            
            return streamReader.ReadToEnd();
        }

        public static void WriteString(this Stream stream, string str, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;

            var bytes = encoding.GetBytes(str);
            var ms = new MemoryStream(bytes);
            ms.CopyTo(stream);
        }
    }
}