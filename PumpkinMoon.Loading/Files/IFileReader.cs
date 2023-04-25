using System.IO;

namespace PumpkinMoon.Loading.Files;

public interface IFileReader
{
    Stream ReadFile(string path);
}