using DC_bot.IO;

namespace DC_bot_tests.UnitTests.IO;

[Trait("Category", "Unit")]
public class PhysicalFileSystemTests : IDisposable
{
    private readonly PhysicalFileSystem _fileSystem;
    private readonly string _testDirectory;

    public PhysicalFileSystemTests()
    {
        _fileSystem = new PhysicalFileSystem();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"PhysicalFileSystemTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory)) Directory.Delete(_testDirectory, true);
    }

    #region DirectoryExists Tests

    [Fact]
    public void DirectoryExists_DirectoryExists_ReturnsTrue()
    {
        var testDir = Path.Combine(_testDirectory, "existing_dir");
        Directory.CreateDirectory(testDir);

        var result = _fileSystem.DirectoryExists(testDir);

        Assert.True(result);
    }

    [Fact]
    public void DirectoryExists_DirectoryDoesNotExist_ReturnsFalse()
    {
        var testDir = Path.Combine(_testDirectory, "nonexistent_dir");

        var result = _fileSystem.DirectoryExists(testDir);

        Assert.False(result);
    }

    [Fact]
    public void DirectoryExists_FilePathGiven_ReturnsFalse()
    {
        var filePath = Path.Combine(_testDirectory, "test.txt");
        File.WriteAllText(filePath, "test content");

        var result = _fileSystem.DirectoryExists(filePath);

        Assert.False(result);
    }

    #endregion

    #region CreateDirectory Tests

    [Fact]
    public void CreateDirectory_NewDirectory_CreatesSuccessfully()
    {
        var newDir = Path.Combine(_testDirectory, "new_directory");

        _fileSystem.CreateDirectory(newDir);

        Assert.True(Directory.Exists(newDir));
    }

    [Fact]
    public void CreateDirectory_NestedDirectories_CreatesAll()
    {
        var nestedDir = Path.Combine(_testDirectory, "level1", "level2", "level3");

        _fileSystem.CreateDirectory(nestedDir);

        Assert.True(Directory.Exists(nestedDir));
    }

    [Fact]
    public void CreateDirectory_DirectoryAlreadyExists_DoesNotThrow()
    {
        var existingDir = Path.Combine(_testDirectory, "existing");
        Directory.CreateDirectory(existingDir);

        _fileSystem.CreateDirectory(existingDir);
        Assert.True(Directory.Exists(existingDir));
    }

    #endregion

    #region FileExists Tests

    [Fact]
    public void FileExists_FileExists_ReturnsTrue()
    {
        var filePath = Path.Combine(_testDirectory, "test.txt");
        File.WriteAllText(filePath, "content");

        var result = _fileSystem.FileExists(filePath);

        Assert.True(result);
    }

    [Fact]
    public void FileExists_FileDoesNotExist_ReturnsFalse()
    {
        var filePath = Path.Combine(_testDirectory, "nonexistent.txt");

        var result = _fileSystem.FileExists(filePath);

        Assert.False(result);
    }

    [Fact]
    public void FileExists_DirectoryPathGiven_ReturnsFalse()
    {
        var dirPath = Path.Combine(_testDirectory, "directory");
        Directory.CreateDirectory(dirPath);

        var result = _fileSystem.FileExists(dirPath);

        Assert.False(result);
    }

    #endregion

    #region ReadAllText Tests

    [Fact]
    public void ReadAllText_FileExists_ReadsContent()
    {
        var filePath = Path.Combine(_testDirectory, "test.txt");
        const string content = "Hello, World!";
        File.WriteAllText(filePath, content);

        var result = _fileSystem.ReadAllText(filePath);

        Assert.Equal(content, result);
    }

    [Fact]
    public void ReadAllText_FileDoesNotExist_ThrowsFileNotFoundException()
    {
        var filePath = Path.Combine(_testDirectory, "nonexistent.txt");

        Assert.Throws<FileNotFoundException>(() => _fileSystem.ReadAllText(filePath));
    }

    [Fact]
    public void ReadAllText_EmptyFile_ReturnsEmptyString()
    {
        var filePath = Path.Combine(_testDirectory, "empty.txt");
        File.WriteAllText(filePath, "");

        var result = _fileSystem.ReadAllText(filePath);

        Assert.Empty(result);
    }

    [Fact]
    public void ReadAllText_LargeFile_ReadsAllContent()
    {
        var filePath = Path.Combine(_testDirectory, "large.txt");
        var largeContent = new string('x', 10000);
        File.WriteAllText(filePath, largeContent);

        var result = _fileSystem.ReadAllText(filePath);

        Assert.Equal(largeContent, result);
    }

    [Fact]
    public void ReadAllText_JsonFile_ReadsCorrectly()
    {
        var filePath = Path.Combine(_testDirectory, "data.json");
        const string jsonContent = @"{ ""name"": ""test"", ""value"": 123 }";
        File.WriteAllText(filePath, jsonContent);

        var result = _fileSystem.ReadAllText(filePath);

        Assert.Equal(jsonContent, result);
    }

    #endregion

    #region WriteAllText Tests

    [Fact]
    public void WriteAllText_NewFile_CreatesFile()
    {
        var filePath = Path.Combine(_testDirectory, "new.txt");
        const string content = "New content";

        _fileSystem.WriteAllText(filePath, content);

        Assert.True(File.Exists(filePath));
        Assert.Equal(content, File.ReadAllText(filePath));
    }

    [Fact]
    public void WriteAllText_ExistingFile_Overwrites()
    {
        var filePath = Path.Combine(_testDirectory, "overwrite.txt");
        File.WriteAllText(filePath, "old content");
        const string newContent = "new content";

        _fileSystem.WriteAllText(filePath, newContent);

        Assert.Equal(newContent, File.ReadAllText(filePath));
    }

    [Fact]
    public void WriteAllText_EmptyString_WritesEmptyFile()
    {
        var filePath = Path.Combine(_testDirectory, "empty_write.txt");

        _fileSystem.WriteAllText(filePath, "");

        Assert.True(File.Exists(filePath));
        Assert.Empty(File.ReadAllText(filePath));
    }

    [Fact]
    public void WriteAllText_LargeContent_WritesSuccessfully()
    {
        var filePath = Path.Combine(_testDirectory, "large_write.txt");
        var largeContent = new string('y', 10000);

        _fileSystem.WriteAllText(filePath, largeContent);

        Assert.Equal(largeContent, File.ReadAllText(filePath));
    }

    [Fact]
    public void WriteAllText_JsonContent_WritesValidJson()
    {
        var filePath = Path.Combine(_testDirectory, "data.json");
        const string jsonContent = @"{ ""key"": ""value"", ""number"": 42 }";

        _fileSystem.WriteAllText(filePath, jsonContent);

        var readContent = File.ReadAllText(filePath);
        Assert.Equal(jsonContent, readContent);
    }

    [Fact]
    public void WriteAllText_DirectoryDoesNotExist_ThrowsException()
    {
        var filePath = Path.Combine(_testDirectory, "nonexistent_dir", "file.txt");

        Assert.Throws<DirectoryNotFoundException>(() => _fileSystem.WriteAllText(filePath, "content"));
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void FileSystem_WriteAndRead_Content_MatchesSync()
    {
        var filePath = Path.Combine(_testDirectory, "sync_test.txt");
        const string originalContent = "Test content for sync";

        _fileSystem.WriteAllText(filePath, originalContent);
        var readContent = _fileSystem.ReadAllText(filePath);

        Assert.Equal(originalContent, readContent);
    }

    [Fact]
    public void FileSystem_MultipleFiles_HandlesIndependently()
    {
        var file1 = Path.Combine(_testDirectory, "file1.txt");
        var file2 = Path.Combine(_testDirectory, "file2.txt");
        const string content1 = "Content 1";
        const string content2 = "Content 2";

        _fileSystem.WriteAllText(file1, content1);
        _fileSystem.WriteAllText(file2, content2);

        var read1 = _fileSystem.ReadAllText(file1);
        var read2 = _fileSystem.ReadAllText(file2);

        Assert.Equal(content1, read1);
        Assert.Equal(content2, read2);
    }

    #endregion
}
