# Playlist Persistence Contracts

This folder contains saved playlist persistence contracts.

## Why This Folder Exists

Saved playlists are durable user-created collections. They are different from the active playback queue: a playlist is stored metadata plus ordered tracks, while the queue is a runtime playback list.

These contracts keep saved playlist behavior separate from queue behavior and from EF Core entity shape.

## Files

### IPlaylistRepository.cs

Contract for saved playlist metadata operations.

### IPlaylistTrackRepository.cs

Contract for ordered playlist track operations.

## Why Metadata And Tracks Are Split

Playlist metadata operations ask questions like "does this playlist exist?" or "rename this playlist." Track operations mutate ordered rows inside one playlist.

Keeping the contracts split prevents metadata workflows from needing track mutation methods and makes tests easier to target.

## What Belongs Here

- playlist create/delete/rename/read operations
- ordered playlist track reads
- adding/removing playlist tracks

## What Does Not Belong Here

- active queue mutation
- Lavalink search logic
- API request DTOs

## Maintenance Notes

- Playlist names are guild-scoped.
- Track order is represented explicitly through order numbers.
