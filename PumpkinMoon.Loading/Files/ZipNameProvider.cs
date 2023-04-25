using System;
using System.IO;
using PumpkinMoon.Loading.Namespace;

namespace PumpkinMoon.Loading.Files;

public class ZipNameProvider : INamespacedIdProvider
{
    public string ArchiveName;

    public string GetNamespacedId(string path, string root, Type type)
    {
        string filename = Path.GetFileNameWithoutExtension(path);
        string typeName = type.Name;
        string parentDir = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(path));

        if (string.IsNullOrEmpty(parentDir))
        {
            return $"{ArchiveName}.{typeName}.{filename}".Replace(" ", "").ToLower();
        }

        return $"{ArchiveName}.{parentDir}.{typeName}.{filename}".Replace(" ", "").ToLower();
    }
}