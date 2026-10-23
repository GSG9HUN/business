# Playlist

Exposes saved playlist creation, URL import, track editing, and queue loading through slash commands.

## Overview

- DSharpPlus binds interaction options, then each adapter dispatches a SlashCommandExecutionRequest to the shared text command pipeline.
- Keep user input and response presentation here. Playback ownership, track transport, and database transitions belong to the services reached by the command.
- PlaylistSlashCommand groups the playlist operations into one module and quotes names before dispatching commands that expect two values. URL resolution and repository limits remain in the saved-playlist services.

## Classes

### PlaylistSlashCommand

**File:** `DC bot/Commands/SlashCommands/Playlist/PlaylistSlashCommand.cs`

**Purpose:** Adapts the playlist slash command to shared command execution and interaction acknowledgement.

**Methods:**

- `Create` — Adapts the create slash route to its shared handler and interaction response policy.

```csharp
public Task Create(SlashCommandContext context, [Parameter("name")][Description("Playlist name")] string name);
```

- `AddSong` — Adapts the add song slash route to its shared handler and interaction response policy.

```csharp
public Task AddSong(
    SlashCommandContext context,
    [Parameter("name")][Description("Playlist name")] string name,
    [Parameter("url")][Description("Song URL or search query")] string url);
```

- `Delete` — Adapts the delete slash route to its shared handler and interaction response policy.

```csharp
public async Task Delete(
    SlashCommandContext context,
    [Parameter("name")][Description("Playlist name")] string name,
    [Parameter("confirm")][Description("Must be true to delete the playlist")] bool confirm = false);
```

- `List` — Adapts the list slash route to its shared handler and interaction response policy.

```csharp
public Task List(SlashCommandContext context);
```

- `Load` — Adapts the load slash route to its shared handler and interaction response policy.

```csharp
public Task Load(SlashCommandContext context, [Parameter("name")][Description("Playlist name")] string name);
```

- `RemoveSong` — Adapts the remove song slash route to its shared handler and interaction response policy.

```csharp
public Task RemoveSong(
    SlashCommandContext context,
    [Parameter("name")][Description("Playlist name")] string name,
    [Parameter("track-number")][Description("Track number from playlist view")] int trackNumber);
```

**Additional methods:** `Save`, `View`, `Rename`. See the source and its XML contracts for the complete parameter lists.

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await playlistSlashCommand.AddSong(context, name, url);
```

---

## Related Components

- `DC bot/Commands/SlashCommands/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Constants/` — Centralizes identifiers used to select localized messages and validation errors.
- `DC bot/Helper/` — Contains small adapters and utilities used by command and presentation boundaries.
- `DC bot/Interface/Service/Localization/` — Defines translation lookup, formatting, and guild language selection.
- `DC bot/Interface/Service/SlashCommands/` — Connects DSharpPlus interactions to the shared text command pipeline.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
