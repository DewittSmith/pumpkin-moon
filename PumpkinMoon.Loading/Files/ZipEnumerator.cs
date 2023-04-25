using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace PumpkinMoon.Loading.Files;

public class ZipEnumerator : IDirectoryEnumerator
{
    public ZipArchive Archive;

    public IEnumerable<string> EnumerateFiles(string path)
    {
        var result = Archive.Entries.Where(x => Path.HasExtension(x.Name)).Select(x => x.FullName);
        return result;
    }

    public IEnumerable<string> EnumerateDirectories(string path)
    {
        yield break;
    }
}