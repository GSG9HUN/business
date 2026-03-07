namespace DC_bot.Interface.Service.IO;

public interface IFileSystem
{
    bool DirectoryExists(string path);
    void CreateDirectory(string path);
    bool FileExists(string path);
    string ReadAllText(string path);
    void WriteAllText(string path, string contents);
}

