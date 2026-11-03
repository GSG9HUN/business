using DC_bot.Interface.Service.IO;

namespace DC_bot.IO;

/// <summary>
/// Implements the file-system boundary using local directories and text files.
/// </summary>
public sealed class PhysicalFileSystem : IFileSystem
{

    /// <summary>
    /// Checks whether the supplied directory path exists.
    /// </summary>
    /// <param name="path">The path supplied for this operation.</param>
    /// <returns>Whether the requested check or operation succeeded.</returns>
    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    /// <summary>
    /// Creates a directory required for subsequent file writes.
    /// </summary>
    /// <param name="path">The path supplied for this operation.</param>
    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }

    /// <summary>
    /// Checks whether the supplied file path exists.
    /// </summary>
    /// <param name="path">The path supplied for this operation.</param>
    /// <returns>Whether the requested check or operation succeeded.</returns>
    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    /// <summary>
    /// Reads complete file text through the file-system boundary.
    /// </summary>
    /// <param name="path">The path supplied for this operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    public string ReadAllText(string path)
    {
        return File.ReadAllText(path);
    }

    /// <summary>
    /// Writes complete file text through the file-system boundary.
    /// </summary>
    /// <param name="path">The path supplied for this operation.</param>
    /// <param name="contents">The contents supplied for this operation.</param>
    public void WriteAllText(string path, string contents)
    {
        File.WriteAllText(path, contents);
    }
}
