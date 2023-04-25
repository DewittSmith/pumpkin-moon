using System;
using System.IO;

namespace PumpkinMoon.Loading.Loaders;

public interface ILoader
{
    Lazy LazyLoad(Stream stream, out Type type);
}