using System;

namespace PumpkinMoon.Loading.Namespace;

public interface INamespacedIdProvider
{
    string GetNamespacedId(string path, string root, Type type);
}