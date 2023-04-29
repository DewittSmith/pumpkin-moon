using System;
using System.IO;

namespace PumpkinMoon.Loading.Namespace;

public class ZipNameProvider : INamespacedIdProvider
{
    public string ArchiveName;
    private readonly string namespaceSeparator;

    public ZipNameProvider(string namespaceSeparator = ".")
    {
        this.namespaceSeparator = namespaceSeparator;
    }

    public string GetNamespacedId(string path, string root, Type type)
    {
        string filename = Path.GetFileNameWithoutExtension(path);
        string typeName = type.Name;
        string parentDir = Path.GetFileName(Path.GetDirectoryName(path));

        if (string.IsNullOrEmpty(parentDir))
        {
            return $"{ArchiveName}{namespaceSeparator}{typeName}.{filename}".Replace(" ", "").ToLower();
        }

        return $"{ArchiveName}{namespaceSeparator}{parentDir}.{typeName}.{filename}".Replace(" ", "").ToLower();
    }
}