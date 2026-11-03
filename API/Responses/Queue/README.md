# Queue

Defines client-facing output for ordered queue entries, canonical batches, conditional start claims, and bounded playback history.

## Overview

- Handlers, mappers, and snapshot providers construct these records from shared contract values and return them through HTTP or matching realtime events.
- Keep field meaning consistent for mobile consumers. An output model describes observable state and must not contain a tracked entity or a live SDK object.

## Models and Results

### QueueResponse

**File:** `API/Responses/Queue/QueueResponse.cs`

**Purpose:** Carries the queue payload returned to HTTP and mobile clients.

**Record declaration:**

```csharp
public sealed record QueueResponse(string GuildId, int TrackCount, IReadOnlyList<QueueTrackResponse> Tracks);
```

---

### QueueTrackResponse

**File:** `API/Responses/Queue/QueueTrackResponse.cs`

**Purpose:** Carries the queue track payload returned to HTTP and mobile clients.

**Record declaration:**

```csharp
public sealed record QueueTrackResponse(
    int Position,
    string Title,
    string Author,
    int Duration,
    string TrackUri,
    string? ArtworkUri,
    string? RequestedBy);
```

---

## Related Components

- `API/Responses/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
