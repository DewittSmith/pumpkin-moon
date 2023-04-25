using System.IO;
using System.IO.Compression;

namespace PumpkinMoon.Loading.Files;

public class ZipFileReader : IFileReader
{
    public ZipArchive Archive;

    public Stream ReadFile(string path)
    {
        ZipArchiveEntry entry = Archive.GetEntry(path);
        return entry?.Open();
    }
}