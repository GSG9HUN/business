# Payloads

Defines the serialized arguments stored with durable commands submitted by the API and consumed by the bot worker.

## Overview

- Queue payloads describe play, move, and removal input. Playlist payloads describe a saved playlist target, an imported URL, or a track to resolve.
- These records describe work; they do not execute it. Preserve field compatibility across API and worker deployments, and validate input before side effects.

## Models and Results

### PlaylistCommandPayload

**File:** `DC bot.Contracts/BotControl/Payloads/PlaylistCommandPayload.cs`

**Purpose:** Identifies a saved playlist for a worker operation using its durable identifier and display name.

**Record declaration:**

```csharp
public sealed record PlaylistCommandPayload(string PlaylistId, string PlaylistName);
```

---

### PlaylistImportCommandPayload

**File:** `DC bot.Contracts/BotControl/Payloads/PlaylistImportCommandPayload.cs`

**Purpose:** Carries the target playlist name and source URL for a worker to resolve and import.

**Record declaration:**

```csharp
public sealed record PlaylistImportCommandPayload(string Name, string Url);
```

---

### PlaylistTrackCommandPayload

**File:** `DC bot.Contracts/BotControl/Payloads/PlaylistTrackCommandPayload.cs`

**Purpose:** Identifies the target saved playlist and the track URL to resolve before adding a stored track.

**Record declaration:**

```csharp
public sealed record PlaylistTrackCommandPayload(string PlaylistId, string PlaylistName, string SongUrl);
```

---

### QueueEnqueueCommandPayload

**File:** `DC bot.Contracts/BotControl/Payloads/QueueEnqueueCommandPayload.cs`

**Purpose:** Carries a play query, optional search mode, requester display value, and optional voice/text channel identifiers for the worker execution context.

**Record declaration:**

```csharp
public sealed record QueueEnqueueCommandPayload(
    string Query,
    string? SearchMode,
    string? RequestedBy,
    ulong? VoiceChannelId = null,
    ulong? TextChannelId = null);
```

---

### QueueMoveToIndexCommandPayload

**File:** `DC bot.Contracts/BotControl/Payloads/QueueMoveToIndexCommandPayload.cs`

**Purpose:** Carries source and destination queue indices for a move command; this is distinct from a one-based displayed track number.

**Record declaration:**

```csharp
public sealed record QueueMoveToIndexCommandPayload(int TrackIndex, int TargetIndex);
```

---

### QueueRemoveCommandPayload

**File:** `DC bot.Contracts/BotControl/Payloads/QueueRemoveCommandPayload.cs`

**Purpose:** Carries the displayed track number selected for removal from the waiting queue.

**Record declaration:**

```csharp
public sealed record QueueRemoveCommandPayload(int TrackNumber);
```

---

## Related Components

- `DC bot.Contracts/BotControl/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/README.md` — Project setup and architecture.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
