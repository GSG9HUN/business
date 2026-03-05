# IO (Input/Output)

This folder contains file system abstractions and I/O operations.

## What's here?

File system abstraction layer that:
- Wraps .NET file operations
- Enables testability (no real files in tests)
- Provides a mockable interface
- Centralizes I/O logic

## Contents

### PhysicalFileSystem.cs

Implementation of `IFileSystem` that wraps standard .NET file operations.

```csharp
public sealed class PhysicalFileSystem : IFileSystem
{
    public bool DirectoryExists(string path) 
        => Directory.Exists(path);

    public void CreateDirectory(string path) 
        => Directory.CreateDirectory(path);

    public bool FileExists(string path) 
        => File.Exists(path);

    public string ReadAllText(string path) 
        => File.ReadAllText(path);

    public void WriteAllText(string path, string contents) 
        => File.WriteAllText(path, contents);
}
```

## Why Abstract the File System?

### Without Abstraction (❌ Bad)
```csharp
public class MusicQueueService
{
    public void SaveQueue(ulong guildId, Queue<Track> queue)
    {
        // Direct file system access
        var path = $"queues/{guildId}.json";
        File.WriteAllText(path, JsonSerializer.Serialize(queue)); // Hard to test!
    }
}

// Testing requires:
// 1. Real file system access
// 2. Cleanup after tests
// 3. Potential race conditions
// 4. Slower tests
```

### With Abstraction (✅ Good)
```csharp
public class MusicQueueService(IFileSystem fileSystem)
{
    public void SaveQueue(ulong guildId, Queue<Track> queue)
    {
        var path = $"queues/{guildId}.json";
        fileSystem.WriteAllText(path, JsonSerializer.Serialize(queue)); // Mockable!
    }
}

// Testing:
var mockFileSystem = new Mock<IFileSystem>();
var service = new MusicQueueService(mockFileSystem.Object);
service.SaveQueue(123, queue);

// Verify file operations without touching disk
mockFileSystem.Verify(x => x.WriteAllText(
    It.IsAny<string>(), 
    It.IsAny<string>()), 
    Times.Once);
```

## Usage Examples

### Writing Files
```csharp
public class MusicQueueService(IFileSystem fileSystem)
{
    public void SaveQueue(ulong guildId, Queue<ILavaLinkTrack> queue)
    {
        var directory = "guildFiles/queues";
        
        // Ensure directory exists
        if (!fileSystem.DirectoryExists(directory))
        {
            fileSystem.CreateDirectory(directory);
        }

        // Serialize and write
        var path = Path.Combine(directory, $"{guildId}.json");
        var json = JsonSerializer.Serialize(queue);
        fileSystem.WriteAllText(path, json);
    }
}
```

### Reading Files
```csharp
public class LocalizationService(IFileSystem fileSystem)
{
    public Dictionary<string, string> LoadLanguage(string languageCode)
    {
        var path = $"localization/{languageCode}.json";
        
        // Check if file exists
        if (!fileSystem.FileExists(path))
        {
            throw new FileNotFoundException($"Language file not found: {path}");
        }

        // Read and deserialize
        var json = fileSystem.ReadAllText(path);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
    }
}
```

### Conditional Logic
```csharp
public class ConfigurationService(IFileSystem fileSystem)
{
    public void EnsureConfigExists(string path, string defaultContent)
    {
        // Check if already exists
        if (fileSystem.FileExists(path))
        {
            return; // Already exists
        }

        // Create directory if needed
        var directory = Path.GetDirectoryName(path);
        if (!fileSystem.DirectoryExists(directory))
        {
            fileSystem.CreateDirectory(directory);
        }

        // Write default content
        fileSystem.WriteAllText(path, defaultContent);
    }
}
```

## Testing with Mock File System

### Basic Mock
```csharp
[Fact]
public void SaveQueue_WritesToCorrectPath()
{
    // Arrange
    var mockFileSystem = new Mock<IFileSystem>();
    var service = new MusicQueueService(mockFileSystem.Object);
    var guildId = 123456789UL;

    // Act
    service.SaveQueue(guildId, new Queue<Track>());

    // Assert
    mockFileSystem.Verify(x => x.WriteAllText(
        It.Is<string>(p => p.EndsWith($"{guildId}.json")),
        It.IsAny<string>()),
        Times.Once);
}
```

### In-Memory File System
For more complex testing, create an in-memory implementation:

```csharp
public class InMemoryFileSystem : IFileSystem
{
    private readonly Dictionary<string, string> _files = new();
    private readonly HashSet<string> _directories = new();

    public bool DirectoryExists(string path) 
        => _directories.Contains(path);

    public void CreateDirectory(string path) 
        => _directories.Add(path);

    public bool FileExists(string path) 
        => _files.ContainsKey(path);

    public string ReadAllText(string path)
    {
        if (!_files.TryGetValue(path, out var content))
            throw new FileNotFoundException();
        return content;
    }

    public void WriteAllText(string path, string contents) 
        => _files[path] = contents;
}
```

**Usage:**
```csharp
[Fact]
public void LoadLanguage_ReadsFromFileSystem()
{
    // Arrange
    var fileSystem = new InMemoryFileSystem();
    fileSystem.WriteAllText("localization/eng.json", "{ \"key\": \"value\" }");
    var service = new LocalizationService(fileSystem);

    // Act
    var result = service.LoadLanguage("eng");

    // Assert
    Assert.Equal("value", result["key"]);
}
```

## Registration

Register in `Program.cs`:

```csharp
// Production - real file system
services.AddSingleton<IFileSystem, PhysicalFileSystem>();

// Testing - in-memory file system
services.AddSingleton<IFileSystem, InMemoryFileSystem>();
```

## Error Handling

File operations can fail. Handle appropriately:

```csharp
public class QueuePersistenceService(IFileSystem fileSystem, ILogger<QueuePersistenceService> logger)
{
    public bool TrySaveQueue(ulong guildId, Queue<Track> queue)
    {
        try
        {
            var path = $"queues/{guildId}.json";
            var json = JsonSerializer.Serialize(queue);
            fileSystem.WriteAllText(path, json);
            return true;
        }
        catch (IOException ex)
        {
            logger.LogError(ex, "Failed to save queue for guild {GuildId}", guildId);
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogError(ex, "Permission denied saving queue for guild {GuildId}", guildId);
            return false;
        }
    }
}
```

## File Paths

Use `Path.Combine` for cross-platform compatibility:

```csharp
// ✅ Good - works on Windows, Linux, macOS
var path = Path.Combine("guildFiles", "queues", $"{guildId}.json");
// Result: "guildFiles/queues/123456789.json" (Unix)
// Result: "guildFiles\queues\123456789.json" (Windows)

// ❌ Bad - hardcoded separators
var path = $"guildFiles/queues/{guildId}.json"; // Breaks on Windows with \
```

## Future Extensions

`IFileSystem` can be extended for:
- Async operations (`ReadAllTextAsync`, `WriteAllTextAsync`)
- Stream operations (`OpenRead`, `OpenWrite`)
- File metadata (`GetLastWriteTime`, `GetFileSize`)
- Directory enumeration (`GetFiles`, `GetDirectories`)

```csharp
public interface IFileSystem
{
    // Existing sync methods
    bool FileExists(string path);
    string ReadAllText(string path);
    void WriteAllText(string path, string contents);

    // Future async methods
    Task<string> ReadAllTextAsync(string path, CancellationToken ct = default);
    Task WriteAllTextAsync(string path, string contents, CancellationToken ct = default);

    // Stream methods
    Stream OpenRead(string path);
    Stream OpenWrite(string path);

    // Directory operations
    string[] GetFiles(string directory, string searchPattern = "*");
}
```

## Best Practices

- ✅ Use `IFileSystem` interface, not `File`/`Directory` directly
- ✅ Use `Path.Combine` for path construction
- ✅ Ensure directories exist before writing files
- ✅ Handle I/O exceptions appropriately
- ✅ Use async I/O for large files (future enhancement)
- ✅ Log file operations for debugging
- ❌ Don't hard-code file paths (use configuration)
- ❌ Don't forget to dispose streams (use `using`)
- ❌ Don't catch all exceptions (be specific)

## Related

- **Interface/IFileSystem.cs** - File system contract
- **Service/MusicServices/MusicQueueService.cs** - Uses file system for queue persistence
- **Service/LocalizationService.cs** - Uses file system for language files
- **guildFiles/** - Persistent data directory

