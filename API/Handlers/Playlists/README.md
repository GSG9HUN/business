# Playlist Handlers

This folder contains handlers for saved playlist API endpoints.

## Files

### PlaylistHandlers.cs

Handles playlist create, list, view, rename, delete, save, and track mutation requests.

## Notes

- Keep HTTP request/response mapping here.
- Saved playlist domain behavior belongs in playlist services and persistence repositories.
- Loading a playlist into active playback should enqueue bot-control behavior rather than directly controlling Lavalink from the API.
