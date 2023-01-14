using System.Collections.Generic;

namespace PumpkinMoon.Loading.Files
{
    public interface IDirectoryEnumerator
    {
        IEnumerable<string> EnumerateFiles(string path);
        IEnumerable<string> EnumerateDirectories(string path);
    }
}