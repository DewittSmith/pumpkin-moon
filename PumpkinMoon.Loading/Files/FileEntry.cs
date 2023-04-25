namespace PumpkinMoon.Loading.Files;

internal readonly struct FileEntry
{
    public readonly string Root;
    public readonly string Path;
    public readonly string Extension;

    public FileEntry(string root, string path)
    {
        Root = root;
        Path = path;
        Extension = System.IO.Path.GetExtension(path).ToLower();
    }
}