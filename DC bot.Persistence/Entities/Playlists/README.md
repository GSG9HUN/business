# Playlist Entities

This folder contains saved playlist EF Core entities.

## Why This Folder Exists

Saved playlists are durable collections, not active playback queues. These entities represent playlist metadata and ordered track rows as EF Core table models.

## Files

### PlaylistEntity.cs

Saved playlist metadata owned by a guild.

### PlaylistTrackEntity.cs

Ordered track row inside a saved playlist.

## What The Entities Represent

`PlaylistEntity` stores the playlist identity and ownership. `PlaylistTrackEntity` stores track identifiers and order numbers inside that playlist.

## Maintenance Notes

- Playlist tracks are ordered by `OrderNumber`.
- Playlist tracks are cascade-deleted with their parent playlist.
