# Persistence Service Interfaces

This folder contains persistence contracts used by the service layer.

## Interfaces

- `IGuildDataRepository` - guild premium state operations
- `IPlaybackStateRepository` - current track and repeat flags
- `IQueueRepository` - queue item lifecycle and ordering
- `IRepeatListRepository` - repeat-list persistence

## Models

See `Models/` for immutable record types used in interface method signatures.

## Design Intent

- service layer depends on these abstractions, not EF Core details
- implementations live in `Persistence/Repositories/`
- contracts use `ulong` guild identifiers to match Discord domain objects
