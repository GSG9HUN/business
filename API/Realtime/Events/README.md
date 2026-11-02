# Events

Defines immutable command-progress and guild snapshot messages sent to mobile clients.

## Overview

- Publishers attach repository-derived values or REST-compatible snapshots and send both feature-specific and generic event names where applicable.
- Payloads describe observable state. They are not instructions to execute a bot command and should not contain EF entities or Discord SDK objects.

## Models and Results

### BotControlCommandEvent

**File:** `API/Realtime/Events/BotControlCommandEvent.cs`

**Purpose:** Carries the bot control command payload sent to authorized mobile realtime subscribers.

**Record declaration:**

```csharp
public sealed record BotControlCommandEvent(
    string CommandId,
    string GuildId,
    string UserId,
    string Type,
    string State,
    string? ErrorMessage,
    string? ResultJson,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ClaimedAtUtc,
    DateTimeOffset? CompletedAtUtc);
```

---

### GuildBotStatusEvent

**File:** `API/Realtime/Events/GuildBotStatusEvent.cs`

**Purpose:** Carries the guild bot status payload sent to authorized mobile realtime subscribers.

**Record declaration:**

```csharp
public sealed record GuildBotStatusEvent(
    string GuildId,
    string EventName,
    DateTimeOffset UpdatedAtUtc,
    GuildBotStatusResponse Snapshot);
```

---

### PlaybackSnapshotEvent

**File:** `API/Realtime/Events/PlaybackSnapshotEvent.cs`

**Purpose:** Carries the playback snapshot payload sent to authorized mobile realtime subscribers.

**Record declaration:**

```csharp
public sealed record PlaybackSnapshotEvent(
    string GuildId,
    string EventName,
    DateTimeOffset UpdatedAtUtc,
    PlaybackStatusResponse Snapshot);
```

---

### QueueSnapshotEvent

**File:** `API/Realtime/Events/QueueSnapshotEvent.cs`

**Purpose:** Carries the queue snapshot payload sent to authorized mobile realtime subscribers.

**Record declaration:**

```csharp
public sealed record QueueSnapshotEvent(string GuildId, string EventName, DateTimeOffset UpdatedAtUtc, QueueResponse Snapshot);
```

---

## Related Components

- `API/Realtime/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `API/Responses/Guilds/` — Defines client-facing output for guild defaults, premium state, and the mapped premium-audit schema.
- `API/Responses/Playback/` — Defines client-facing output for current playback state, repeat flags, start ownership, and canonical batch metadata.
- `API/Responses/Queue/` — Defines client-facing output for ordered queue entries, canonical batches, conditional start claims, and bounded playback history.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
