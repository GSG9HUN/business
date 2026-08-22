# Playlist Requests

This folder contains request DTOs for playlist operations.

## Why This Folder Exists

Playlist endpoints receive small pieces of user input: playlist names, external URLs, and track identifiers. These DTOs describe that HTTP input without mixing it with playlist persistence models or music service behavior.

## Files

### AddSongRequest.cs

Carries a song URL or identifier to add to a playlist.

### CreatePlaylistRequest.cs

Carries the playlist name for create operations.

### RenamePlaylistRequest.cs

Carries the replacement playlist name.

### SavePlaylistRequest.cs

Carries a playlist name and external URL for save/import operations.

## Flow Context

Handlers should validate these payloads, then pass clean values to playlist services or bot-control command flow. The DTOs should not perform validation, persistence, or track lookup themselves.

## Maintenance Notes

- Playlist names should be validated before reaching persistence.
- Request DTOs should not call playlist services directly.
