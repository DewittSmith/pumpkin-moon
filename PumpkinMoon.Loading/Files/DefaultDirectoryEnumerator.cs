using System.Collections.Generic;
using System.IO;

namespace PumpkinMoon.Loading.Files;

public class DefaultDirectoryEnumerator : IDirectoryEnumerator
{
    public IEnumerable<string> EnumerateFiles(string path)
    {
        return Directory.EnumerateFiles(path);
    }

    public IEnumerable<string> EnumerateDirectories(string path)
    {
        return Directory.EnumerateDirectories(path);
    }
}