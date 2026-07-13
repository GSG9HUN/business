# Persistence Interface Models

This folder contains immutable record models returned by persistence contracts.

## Files

- `PlaybackStateRecord.cs`
- `PlaylistRecord.cs`
- `PlaylistSummaryRecord.cs`
- `PlaylistTrackCreateRecord.cs`
- `PlaylistTrackRecord.cs`
- `QueueItemRecord.cs`
- `QueueItemState.cs`

## Purpose

These records decouple service logic from EF Core entities and provide stable, test-friendly data contracts.

## Notes

- `GuildId` is represented as `ulong` at contract level.
- `PlaybackStateRecord.QueueItemId` links current playback to a persisted queue item when available.
- Playlist records are used by `PlaylistService` so EF Core entities do not leak into service or command code.
- `PlaylistSummaryRecord.TrackCount` supports `listPlaylists` responses without loading every track.
- Queue item `State` uses the explicit `QueueItemState` enum at contract level.
- EF Core maps `QueueItemState` to the existing `short` database column, so repository/service code does not pass raw numeric state values.
