# Tracks

Adapts Lavalink tracks to the persisted identity format used by queue and playlist storage.

## Overview

- LavalinkTrackSerializer implements ITrackSerializer so use cases do not duplicate parsing or encoding details.
- Treat serialized track identity as a storage boundary. Event correlation and semantic track matching belong to the session/event logic that consumes it.

## Classes

### LavalinkTrackSerializer

**File:** `DC bot/Service/Music/Tracks/LavalinkTrackSerializer.cs`

**Purpose:** Serializes track identities and restores tracks with their optional queue-item identity.

**Implements / inherits:** `ITrackSerializer`.

**Methods:**

- `Deserialize` — Restores an encoded track and attaches the optional durable queue-item identity.

```csharp
public ILavaLinkTrack Deserialize(string trackIdentifier, long? queueItemId = null);
```

- `Serialize` — Encodes a track for durable storage independently of its in-memory wrapper identity.

```csharp
public string Serialize(ILavaLinkTrack track);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = lavalinkTrackSerializer.Deserialize(trackIdentifier, queueItemId);
```

---

## Related Components

- `DC bot/Service/Music/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Wrapper/Lavalink/` — Exposes Lavalink track metadata through ILavaLinkTrack.
- `DC bot/Interface/Service/Music/Tracks/` — Defines the tracks boundary used by the bot music pipeline.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
