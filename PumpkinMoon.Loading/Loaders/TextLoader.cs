using System;
using System.IO;

namespace PumpkinMoon.Loading.Loaders
{
    public class TextLoader : ILoader
    {
        public Type Type => typeof(string);

        public Lazy LazyLoad(string namespacedId, Stream stream)
        {
            return new Lazy(() =>
            {
                using TextReader textReader = new StreamReader(stream);
                string result = textReader.ReadToEnd();
                stream.Dispose();

                return result;
            });
        }
    }
}