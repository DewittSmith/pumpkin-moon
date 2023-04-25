using System;
using System.IO;

namespace PumpkinMoon.Loading.Loaders;

public class TextLoader : ILoader
{
    public Lazy LazyLoad(Stream stream, out Type type)
    {
        type = typeof(string);

        return new Lazy(() =>
        {
            using TextReader textReader = new StreamReader(stream);
            string result = textReader.ReadToEnd();
            stream.Dispose();

            return result;
        });
    }
}