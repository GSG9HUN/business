# IO Service Interfaces

This folder contains file system service interfaces.

## Files

### IFileSystem.cs

**Purpose:** File system abstraction for testability.

```csharp
public interface IFileSystem
{
    bool DirectoryExists(string path);
    void CreateDirectory(string path);
    bool FileExists(string path);
    string ReadAllText(string path);
    void WriteAllText(string path, string contents);
}
```

**Methods:**

- `DirectoryExists()` - Check if directory exists
- `CreateDirectory()` - Create directory
- `FileExists()` - Check if file exists
- `ReadAllText()` - Read file contents
- `WriteAllText()` - Write file contents

**Implementation:** `IO/PhysicalFileSystem.cs`

**Used By:**

- `Service/LocalizationService.cs` - Read/write language files
- Tests can mock this interface or use `PhysicalFileSystem` against a temporary directory

**Benefits:**

- Testable through mocks or temporary filesystem fixtures
- Isolates I/O operations
- Simplifies mocking in tests

---

## Related Components

- **IO/PhysicalFileSystem.cs** - Production implementation
- **DC bot tests/UnitTests/IO/PhysicalFileSystemTests.cs** - Production implementation tests

