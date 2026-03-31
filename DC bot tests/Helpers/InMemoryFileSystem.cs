using DC_bot.Interface.Service.IO;

namespace DC_bot_tests.Helpers;

public sealed class InMemoryFileSystem : IFileSystem
{
    private readonly HashSet<string> _directories = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _files = new(StringComparer.OrdinalIgnoreCase);

    public bool DirectoryExists(string path)
    {
        return _directories.Contains(Normalize(path));
    }

    public void CreateDirectory(string path)
    {
        var normalized = Normalize(path);
        _directories.Add(normalized);
    }

    public bool FileExists(string path)
    {
        return _files.ContainsKey(Normalize(path));
    }

    public string ReadAllText(string path)
    {
        return _files[Normalize(path)];
    }

    public void WriteAllText(string path, string contents)
    {
        var normalized = Normalize(path);
        var directory = Path.GetDirectoryName(normalized);
        if (!string.IsNullOrEmpty(directory)) _directories.Add(directory);

        _files[normalized] = contents;
    }

    private static string Normalize(string path)
    {
        return path.Replace('\\', '/');
    }
}