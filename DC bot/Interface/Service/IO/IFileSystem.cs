namespace DC_bot.Interface.Service.IO;

/// <summary>
/// Defines the file system boundary so callers are independent of its concrete implementation.
/// </summary>
public interface IFileSystem
{

    /// <summary>
    /// Checks whether the supplied directory path exists.
    /// </summary>
    /// <param name="path">The path supplied for this operation.</param>
    /// <returns>Whether the requested check or operation succeeded.</returns>
    bool DirectoryExists(string path);

    /// <summary>
    /// Creates a directory required for subsequent file writes.
    /// </summary>
    /// <param name="path">The path supplied for this operation.</param>
    void CreateDirectory(string path);

    /// <summary>
    /// Checks whether the supplied file path exists.
    /// </summary>
    /// <param name="path">The path supplied for this operation.</param>
    /// <returns>Whether the requested check or operation succeeded.</returns>
    bool FileExists(string path);

    /// <summary>
    /// Reads complete file text through the file-system boundary.
    /// </summary>
    /// <param name="path">The path supplied for this operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    string ReadAllText(string path);

    /// <summary>
    /// Writes complete file text through the file-system boundary.
    /// </summary>
    /// <param name="path">The path supplied for this operation.</param>
    /// <param name="contents">The contents supplied for this operation.</param>
    void WriteAllText(string path, string contents);
}
