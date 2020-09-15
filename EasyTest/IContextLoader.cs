using System;
using System.IO;

namespace EasyTest
{
    public interface IContextLoader<out TContext> where TContext : TestContext, new()
    {
        public IContextLoader<TContext> WithDeserializer<T>(Func<Stream, T> deserializer);
        public TContext LoadFromDirectory(string directory);
    }
}