# IO

Abstracts the file operations required by localization and guild preference storage.

## Overview

- PhysicalFileSystem supplies the production implementation; callers receive the contract through DI.
- Keep path selection and localization semantics with their owning service rather than hard-coding them into every file adapter.

## Interfaces

### IFileSystem

**File:** `DC bot/Interface/Service/IO/IFileSystem.cs`

**Purpose:** Defines the file system boundary so callers are independent of its concrete implementation.

**Methods:**

- `CreateDirectory` — Creates a directory required for subsequent file writes.

```csharp
void CreateDirectory(string path);
```

- `DirectoryExists` — Checks whether the supplied directory path exists.

```csharp
bool DirectoryExists(string path);
```

- `FileExists` — Checks whether the supplied file path exists.

```csharp
bool FileExists(string path);
```

- `ReadAllText` — Reads complete file text through the file-system boundary.

```csharp
string ReadAllText(string path);
```

- `WriteAllText` — Writes complete file text through the file-system boundary.

```csharp
void WriteAllText(string path, string contents);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
fileSystem.CreateDirectory(path);
```

---

## Related Components

- `DC bot/Interface/Service/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
