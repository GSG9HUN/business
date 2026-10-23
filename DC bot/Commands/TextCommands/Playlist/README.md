# Playlist

Exposes saved playlist creation, URL import, track editing, and queue loading through text commands.

## Overview

- CommandHandlerService selects an ICommand by name. The command parses input, validates the required context, calls injected services, and sends a localized result.
- Keep user input and response presentation here. Playback ownership, track transport, and database transitions belong to the services reached by the command.
- Playlist names containing spaces can be quoted. Two-value commands such as savePlaylist, addSong, removeSong, and renamePlaylist use the shared argument parser; their slash adapters prepare the same argument shape.

## Classes

### AddSongToPlaylistCommand

**File:** `DC bot/Commands/TextCommands/Playlist/AddSongToPlaylistCommand.cs`

**Purpose:** Handles the add song to playlist text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `addSong`. Message routing applies the configured prefix.

**Properties:**

```csharp
public string Name { get; }
public string Description { get; }
```

**Methods:**

- `ExecuteAsync` — Validates the message context, performs this command's action, and sends its localized outcome.

```csharp
public async Task ExecuteAsync(IDiscordMessage message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await addSongToPlaylistCommand.ExecuteAsync(message);
```

---

### CreatePlaylistCommand

**File:** `DC bot/Commands/TextCommands/Playlist/CreatePlaylistCommand.cs`

**Purpose:** Handles the create playlist text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `createPlaylist`. Message routing applies the configured prefix.

**Properties:**

```csharp
public string Name { get; }
public string Description { get; }
```

**Methods:**

- `ExecuteAsync` — Validates the message context, performs this command's action, and sends its localized outcome.

```csharp
public async Task ExecuteAsync(IDiscordMessage message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await createPlaylistCommand.ExecuteAsync(message);
```

---

### DeletePlaylistCommand

**File:** `DC bot/Commands/TextCommands/Playlist/DeletePlaylistCommand.cs`

**Purpose:** Handles the delete playlist text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `deletePlaylist`. Message routing applies the configured prefix.

**Properties:**

```csharp
public string Name { get; }
public string Description { get; }
```

**Methods:**

- `ExecuteAsync` — Validates the message context, performs this command's action, and sends its localized outcome.

```csharp
public async Task ExecuteAsync(IDiscordMessage message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await deletePlaylistCommand.ExecuteAsync(message);
```

---

### ListPlaylistsCommand

**File:** `DC bot/Commands/TextCommands/Playlist/ListPlaylistsCommand.cs`

**Purpose:** Handles the list playlists text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `listPlaylists`. Message routing applies the configured prefix.

**Properties:**

```csharp
public string Name { get; }
public string Description { get; }
```

**Methods:**

- `ExecuteAsync` — Validates the message context, performs this command's action, and sends its localized outcome.

```csharp
public async Task ExecuteAsync(IDiscordMessage message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await listPlaylistsCommand.ExecuteAsync(message);
```

---

### LoadPlaylistCommand

**File:** `DC bot/Commands/TextCommands/Playlist/LoadPlaylistCommand.cs`

**Purpose:** Handles the load playlist text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `loadPlaylist`. Message routing applies the configured prefix.

**Properties:**

```csharp
public string Name { get; }
public string Description { get; }
```

**Methods:**

- `ExecuteAsync` — Validates the message context, performs this command's action, and sends its localized outcome.

```csharp
public async Task ExecuteAsync(IDiscordMessage message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await loadPlaylistCommand.ExecuteAsync(message);
```

---

### RemoveSongFromPlaylistCommand

**File:** `DC bot/Commands/TextCommands/Playlist/RemoveSongFromPlaylistCommand.cs`

**Purpose:** Handles the remove song from playlist text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `removeSong`. Message routing applies the configured prefix.

**Properties:**

```csharp
public string Name { get; }
public string Description { get; }
```

**Methods:**

- `ExecuteAsync` — Validates the message context, performs this command's action, and sends its localized outcome.

```csharp
public async Task ExecuteAsync(IDiscordMessage message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await removeSongFromPlaylistCommand.ExecuteAsync(message);
```

---

### RenamePlaylistCommand

**File:** `DC bot/Commands/TextCommands/Playlist/RenamePlaylistCommand.cs`

**Purpose:** Handles the rename playlist text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `renamePlaylist`. Message routing applies the configured prefix.

**Properties:**

```csharp
public string Name { get; }
public string Description { get; }
```

**Methods:**

- `ExecuteAsync` — Validates the message context, performs this command's action, and sends its localized outcome.

```csharp
public async Task ExecuteAsync(IDiscordMessage message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await renamePlaylistCommand.ExecuteAsync(message);
```

---

### SavePlaylistCommand

**File:** `DC bot/Commands/TextCommands/Playlist/SavePlaylistCommand.cs`

**Purpose:** Handles the save playlist text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `savePlaylist`. Message routing applies the configured prefix.

**Properties:**

```csharp
public string Name { get; }
public string Description { get; }
```

**Methods:**

- `ExecuteAsync` — Validates the message context, performs this command's action, and sends its localized outcome.

```csharp
public async Task ExecuteAsync(IDiscordMessage message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await savePlaylistCommand.ExecuteAsync(message);
```

---

### ViewPlaylistCommand

**File:** `DC bot/Commands/TextCommands/Playlist/ViewPlaylistCommand.cs`

**Purpose:** Handles the view playlist text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `viewPlaylist`. Message routing applies the configured prefix.

**Properties:**

```csharp
public string Name { get; }
public string Description { get; }
```

**Methods:**

- `ExecuteAsync` — Validates the message context, performs this command's action, and sends its localized outcome.

```csharp
public async Task ExecuteAsync(IDiscordMessage message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await viewPlaylistCommand.ExecuteAsync(message);
```

---

## Related Components

- `DC bot/Commands/TextCommands/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Constants/` — Centralizes identifiers used to select localized messages and validation errors.
- `DC bot/Helper/` — Contains small adapters and utilities used by command and presentation boundaries.
- `DC bot/Interface/` — Defines command and adapter boundaries used inside the bot executable.
- `DC bot/Logging/` — Defines typed log messages and reusable scopes for bot operations.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
