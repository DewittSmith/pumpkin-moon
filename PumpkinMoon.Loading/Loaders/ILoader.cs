using System;
using System.IO;

namespace PumpkinMoon.Loading.Loaders
{
    public interface ILoader
    {
        Type Type { get; }
        Lazy LazyLoad(string namespacedId, Stream stream);
    }
}