# Playlist Repositories

This folder contains saved playlist repository implementations.

## Why This Folder Exists

Saved playlists are persistent user-managed collections. They need separate repository behavior from the active queue because playlists have metadata, names, track ordering, and rename/delete semantics.

## Files

### PlaylistRepository.cs

Implements `IPlaylistRepository`.

Responsibilities:

- create playlist metadata
- find playlists by guild and name
- list playlist summaries
- delete and rename playlists

### PlaylistTrackRepository.cs

Implements `IPlaylistTrackRepository`.

Responsibilities:

- read ordered playlist tracks
- append one or many tracks
- remove a track by order number
- compact track order after removal

## Flow Context

Playlist commands call these repositories to create, save, view, load, rename, delete, and mutate saved playlists. Loading a playlist into the active queue is handled by service code after reading ordered tracks from persistence.

## Maintenance Notes

- Playlist repositories return contract records instead of EF entities.
- Concurrency-sensitive playlist changes should stay transactional.
