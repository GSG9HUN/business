# Discovery

Contains immutable personal favorite values returned by storage contracts.

## Overview

- Share saved source data without exposing tracked entities or Lavalink SDK objects.

## Models

### FavoriteTrackRecord

**Purpose:** Carries stable favorite ID, serialized source track, display title and artist after owner-scoped repository lookup.

**Properties:**

```csharp
public sealed record FavoriteTrackRecord(long Id, string TrackIdentifier, string Title, string Author);
```

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/Discovery/IFavoriteRepository.cs` - Produces owner-scoped records.
- `DC bot/Service/Music/Discovery/FavoriteService.cs` - Consumes saved tracks through normal playback.
