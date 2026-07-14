# Persistence Service Interfaces

This folder contains persistence contracts used by the service layer.

## Interfaces

- `IGuildDataRepository` - guild row creation and premium state operations
- `IPlaybackStateRepository` - current track and repeat flags
- `IPlaylistRepository` - saved playlist metadata operations
- `IPlaylistTrackRepository` - saved playlist track ordering and mutation operations
- `IQueueRepository` - queue item lifecycle and ordering
- `IRepeatListRepository` - repeat-list snapshot persistence used for saving repeat-list mode state and rehydrating queue playback

`IQueueRepository` uses `QueueItemState` (`Queued`, `Playing`, `Played`, `Skipped`) transitions and exposes atomic claim operations through
`ClaimNextQueuedItemAsync`.

## Models

See `Models/` for immutable record types used in interface method signatures.

## Design Intent

- service layer depends on these abstractions, not EF Core details
- implementations live in `Persistence/Repositories/`
- contracts use `ulong` guild identifiers to match Discord domain objects
- playlist repositories return immutable record models instead of EF entities
