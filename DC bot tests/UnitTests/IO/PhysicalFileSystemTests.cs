using DC_bot.IO;

namespace DC_bot_tests.UnitTests.IO;

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
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    #region DirectoryExists Tests

    [Fact]
    public void DirectoryExists_DirectoryExists_ReturnsTrue()
    {
        // Arrange
        var testDir = Path.Combine(_testDirectory, "existing_dir");
        Directory.CreateDirectory(testDir);

        // Act
        var result = _fileSystem.DirectoryExists(testDir);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void DirectoryExists_DirectoryDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var testDir = Path.Combine(_testDirectory, "nonexistent_dir");

        // Act
        var result = _fileSystem.DirectoryExists(testDir);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void DirectoryExists_FilePathGiven_ReturnsFalse()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.txt");
        File.WriteAllText(filePath, "test content");

        // Act
        var result = _fileSystem.DirectoryExists(filePath);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region CreateDirectory Tests

    [Fact]
    public void CreateDirectory_NewDirectory_CreatesSuccessfully()
    {
        // Arrange
        var newDir = Path.Combine(_testDirectory, "new_directory");

        // Act
        _fileSystem.CreateDirectory(newDir);

        // Assert
        Assert.True(Directory.Exists(newDir));
    }

    [Fact]
    public void CreateDirectory_NestedDirectories_CreatesAll()
    {
        // Arrange
        var nestedDir = Path.Combine(_testDirectory, "level1", "level2", "level3");

        // Act
        _fileSystem.CreateDirectory(nestedDir);

        // Assert
        Assert.True(Directory.Exists(nestedDir));
    }

    [Fact]
    public void CreateDirectory_DirectoryAlreadyExists_DoesNotThrow()
    {
        // Arrange
        var existingDir = Path.Combine(_testDirectory, "existing");
        Directory.CreateDirectory(existingDir);

        // Act & Assert
        _fileSystem.CreateDirectory(existingDir); // Should not throw
        Assert.True(Directory.Exists(existingDir));
    }

    #endregion

    #region FileExists Tests

    [Fact]
    public void FileExists_FileExists_ReturnsTrue()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.txt");
        File.WriteAllText(filePath, "content");

        // Act
        var result = _fileSystem.FileExists(filePath);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void FileExists_FileDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "nonexistent.txt");

        // Act
        var result = _fileSystem.FileExists(filePath);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void FileExists_DirectoryPathGiven_ReturnsFalse()
    {
        // Arrange
        var dirPath = Path.Combine(_testDirectory, "directory");
        Directory.CreateDirectory(dirPath);

        // Act
        var result = _fileSystem.FileExists(dirPath);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region ReadAllText Tests

    [Fact]
    public void ReadAllText_FileExists_ReadsContent()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.txt");
        const string content = "Hello, World!";
        File.WriteAllText(filePath, content);

        // Act
        var result = _fileSystem.ReadAllText(filePath);

        // Assert
        Assert.Equal(content, result);
    }

    [Fact]
    public void ReadAllText_FileDoesNotExist_ThrowsFileNotFoundException()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "nonexistent.txt");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => _fileSystem.ReadAllText(filePath));
    }

    [Fact]
    public void ReadAllText_EmptyFile_ReturnsEmptyString()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "empty.txt");
        File.WriteAllText(filePath, "");

        // Act
        var result = _fileSystem.ReadAllText(filePath);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ReadAllText_LargeFile_ReadsAllContent()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "large.txt");
        var largeContent = new string('x', 10000);
        File.WriteAllText(filePath, largeContent);

        // Act
        var result = _fileSystem.ReadAllText(filePath);

        // Assert
        Assert.Equal(largeContent, result);
    }

    [Fact]
    public void ReadAllText_JsonFile_ReadsCorrectly()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "data.json");
        const string jsonContent = @"{ ""name"": ""test"", ""value"": 123 }";
        File.WriteAllText(filePath, jsonContent);

        // Act
        var result = _fileSystem.ReadAllText(filePath);

        // Assert
        Assert.Equal(jsonContent, result);
    }

    #endregion

    #region WriteAllText Tests

    [Fact]
    public void WriteAllText_NewFile_CreatesFile()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "new.txt");
        const string content = "New content";

        // Act
        _fileSystem.WriteAllText(filePath, content);

        // Assert
        Assert.True(File.Exists(filePath));
        Assert.Equal(content, File.ReadAllText(filePath));
    }

    [Fact]
    public void WriteAllText_ExistingFile_Overwrites()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "overwrite.txt");
        File.WriteAllText(filePath, "old content");
        const string newContent = "new content";

        // Act
        _fileSystem.WriteAllText(filePath, newContent);

        // Assert
        Assert.Equal(newContent, File.ReadAllText(filePath));
    }

    [Fact]
    public void WriteAllText_EmptyString_WritesEmptyFile()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "empty_write.txt");

        // Act
        _fileSystem.WriteAllText(filePath, "");

        // Assert
        Assert.True(File.Exists(filePath));
        Assert.Empty(File.ReadAllText(filePath));
    }

    [Fact]
    public void WriteAllText_LargeContent_WritesSuccessfully()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "large_write.txt");
        var largeContent = new string('y', 10000);

        // Act
        _fileSystem.WriteAllText(filePath, largeContent);

        // Assert
        Assert.Equal(largeContent, File.ReadAllText(filePath));
    }

    [Fact]
    public void WriteAllText_JsonContent_WritesValidJson()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "data.json");
        const string jsonContent = @"{ ""key"": ""value"", ""number"": 42 }";

        // Act
        _fileSystem.WriteAllText(filePath, jsonContent);

        // Assert
        var readContent = File.ReadAllText(filePath);
        Assert.Equal(jsonContent, readContent);
    }

    [Fact]
    public void WriteAllText_DirectoryDoesNotExist_ThrowsException()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "nonexistent_dir", "file.txt");

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() => _fileSystem.WriteAllText(filePath, "content"));
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void FileSystem_WriteAndRead_Content_MatchesSync()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "sync_test.txt");
        const string originalContent = "Test content for sync";

        // Act
        _fileSystem.WriteAllText(filePath, originalContent);
        var readContent = _fileSystem.ReadAllText(filePath);

        // Assert
        Assert.Equal(originalContent, readContent);
    }

    [Fact]
    public void FileSystem_MultipleFiles_HandlesIndependently()
    {
        // Arrange
        var file1 = Path.Combine(_testDirectory, "file1.txt");
        var file2 = Path.Combine(_testDirectory, "file2.txt");
        const string content1 = "Content 1";
        const string content2 = "Content 2";

        // Act
        _fileSystem.WriteAllText(file1, content1);
        _fileSystem.WriteAllText(file2, content2);

        var read1 = _fileSystem.ReadAllText(file1);
        var read2 = _fileSystem.ReadAllText(file2);

        // Assert
        Assert.Equal(content1, read1);
        Assert.Equal(content2, read2);
    }

    #endregion
}