# Tracks

Defines the tracks boundary used by the bot music pipeline.

## Overview

- Implementations live under Service/Music/Tracks; commands and orchestrators exchange these contracts instead of constructing transport or persistence dependencies themselves.
- Result states distinguish completed, rejected, pending, and superseded work where applicable. Preserve ownership and cancellation semantics when adding an operation.

## Interfaces

### ILavaLinkTrack

**File:** `DC bot/Interface/Service/Music/Tracks/ILavaLinkTrack.cs`

**Purpose:** Defines the lava link track boundary so callers are independent of its concrete implementation.

**Properties:**

```csharp
// The title of the audio track.
string Title { get; }
string Author { get; }
TimeSpan Duration { get; }
TimeSpan? StartPosition { get; }
Uri? ArtworkUri { get; }
```

**Methods:**

- `ToLavalinkTrack` — Returns the underlying SDK track represented by this adapter.

```csharp
LavalinkTrack ToLavalinkTrack();
```

- `ToString` — Returns the underlying encoded identity or display representation of this adapter.

```csharp
string ToString();
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = lavaLinkTrack.ToLavalinkTrack();
```

---

### ITrackSerializer

**File:** `DC bot/Interface/Service/Music/Tracks/ITrackSerializer.cs`

**Purpose:** Defines the track serializer boundary so callers are independent of its concrete implementation.

**Methods:**

- `Deserialize` — Restores an encoded track and attaches the optional durable queue-item identity.

```csharp
ILavaLinkTrack Deserialize(string trackIdentifier, long? queueItemId = null);
```

- `Serialize` — Encodes a track for durable storage independently of its in-memory wrapper identity.

```csharp
string Serialize(ILavaLinkTrack track);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = trackSerializer.Deserialize(trackIdentifier, queueItemId);
```

---

## Related Components

- `DC bot/Interface/Service/Music/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
