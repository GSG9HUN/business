# IO

Implements the injected file-system boundary for translation and guild preference data.

## Overview

- PhysicalFileSystem performs the local file operations requested through IFileSystem.
- Keep storage paths configurable through their owning code and keep application decisions outside this infrastructure adapter.

## Classes

### PhysicalFileSystem

**File:** `DC bot/IO/PhysicalFileSystem.cs`

**Purpose:** Implements the file-system boundary using local directories and text files.

**Implements / inherits:** `IFileSystem`.

**Methods:**

- `CreateDirectory` — Creates a directory required for subsequent file writes.

```csharp
public void CreateDirectory(string path);
```

- `DirectoryExists` — Checks whether the supplied directory path exists.

```csharp
public bool DirectoryExists(string path);
```

- `FileExists` — Checks whether the supplied file path exists.

```csharp
public bool FileExists(string path);
```

- `ReadAllText` — Reads complete file text through the file-system boundary.

```csharp
public string ReadAllText(string path);
```

- `WriteAllText` — Writes complete file text through the file-system boundary.

```csharp
public void WriteAllText(string path, string contents);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
physicalFileSystem.CreateDirectory(path);
```

---

## Related Components

- `DC bot/README.md` — Parent feature overview and neighboring components.
- `DC bot/Interface/Service/IO/` — Abstracts the file operations required by localization and guild preference storage.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
