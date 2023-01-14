using System;
using System.IO;

namespace PumpkinMoon.Loading.Namespace
{
    public class DefaultNamespaceIdProvider : INamespacedIdProvider
    {
        public string GetNamespacedId(string path, string root, Type type)
        {
            string fileName = Path.GetFileNameWithoutExtension(path).ToLower();
            string typeName = type.Name.ToLower();

            if (string.IsNullOrEmpty(root))
            {
                return $"{typeName}.{fileName}";
            }

            string rootName = Path.GetFileName(root).ToLower();
            return $"{rootName}:{typeName}.{fileName}";
        }
    }
}