using System;
using System.Collections.Generic;

namespace EasyTest
{
    public abstract class TestContent : IDisposable
    {
        internal readonly List<IDisposable> Resources = new List<IDisposable>();

        public void Dispose()
        {
            foreach (var resource in Resources)
            {
                resource.Dispose();
            }
        }
    }
}