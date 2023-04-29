using System;
using System.IO;

namespace PumpkinMoon.Loading.Namespace;

public class DefaultNameProvider : INamespacedIdProvider
{
    private readonly string namespaceSeparator;

    public DefaultNameProvider(string namespaceSeparator = ".")
    {
        this.namespaceSeparator = namespaceSeparator;
    }

    public string GetNamespacedId(string path, string root, Type type)
    {
        string fileName = Path.GetFileNameWithoutExtension(path);
        string typeName = type.Name;
        string parentDir = Path.GetFileName(Path.GetDirectoryName(path));

        if (string.IsNullOrEmpty(parentDir))
        {
            return $"{root}{namespaceSeparator}{typeName}.{fileName}".Replace(" ", "").ToLower();
        }

        string rootName = Path.GetFileName(root);
        return $"{rootName}{namespaceSeparator}{parentDir}.{typeName}.{fileName}".Replace(" ", "").ToLower();
    }
}