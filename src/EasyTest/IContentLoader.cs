using System;
using System.IO;

namespace EasyTest
{
    public interface IContentLoader<out TContent> where TContent : TestContent, new()
    {
        public IContentLoader<TContent> WithDeserializer<T>(Func<Stream, T> deserializer);
        public TContent LoadFromDirectory(string directory);
    }
}