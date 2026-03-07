# IO

This folder contains file system abstraction implementation.

## Files

### PhysicalFileSystem.cs

**Purpose:** Production file system implementation of `IFileSystem` interface.

```csharp
public sealed class PhysicalFileSystem : IFileSystem
{
    public bool DirectoryExists(string path) => Directory.Exists(path);
    public void CreateDirectory(string path) => Directory.CreateDirectory(path);
    public bool FileExists(string path) => File.Exists(path);
    public string ReadAllText(string path) => File.ReadAllText(path);
    public void WriteAllText(string path, string contents) => File.WriteAllText(path, contents);
}
```

**Methods:**
- `DirectoryExists()` - Check if directory exists
- `CreateDirectory()` - Create directory
- `FileExists()` - Check if file exists
- `ReadAllText()` - Read file contents
- `WriteAllText()` - Write file contents

**Usage:**
```csharp
// In LocalizationService
var fileSystem = new PhysicalFileSystem();
var exists = fileSystem.FileExists("localization/eng.json");
var content = fileSystem.ReadAllText(path);
```

**Used By:**
- `Service/LocalizationService.cs` - Language file operations
- `Service/Music/MusicServices/MusicQueueService.cs` - Queue file operations

## Related Components

- **Interface/Service/IO/IFileSystem.cs** - Interface contract
- **Service/LocalizationService.cs** - Uses for language files
- **Service/Music/MusicServices/MusicQueueService.cs** - Uses for queue files
- **DC bot tests/Helpers/InMemoryFileSystem.cs** - Test implementation

