using System.IO;

namespace PumpkinMoon.Loading.Files
{
    public class DefaultFileReader : IFileReader
    {
        public Stream ReadFile(string path)
        {
            return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}