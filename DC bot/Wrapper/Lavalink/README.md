# Lavalink

Exposes Lavalink track metadata through ILavaLinkTrack.

## Overview

- LavaLinkTrackWrapper allows command and presentation code to consume track values without constructing a concrete SDK track.
- Keep serialization in ITrackSerializer and actual playback transport in the playback services.

## Classes

### LavaLinkTrackWrapper

**File:** `DC bot/Wrapper/Lavalink/LavaLinkTrackWrapper.cs`

**Purpose:** Adapts lava link track data and operations to the bot's Discord boundary.

**Implements / inherits:** `ILavaLinkTrack`.

**Properties:**

```csharp
// The associated queue entry identifier, or null when no entry is associated.
public string Title { get; }
public string Author { get; }
public TimeSpan Duration { get; }
public Uri? ArtworkUri { get; }
public TimeSpan? StartPosition { get; }
public long? QueueItemId { get; init; }
```

**Methods:**

- `ToLavalinkTrack` — Returns the underlying SDK track represented by this adapter.

```csharp
public LavalinkTrack ToLavalinkTrack();
```

- `ToString` — Returns the underlying encoded identity or display representation of this adapter.

```csharp
public override string ToString();
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = lavaLinkTrackWrapper.ToLavalinkTrack();
```

---

## Related Components

- `DC bot/Wrapper/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Interface/Service/Music/Tracks/` — Defines the tracks boundary used by the bot music pipeline.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
