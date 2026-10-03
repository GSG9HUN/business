# Player Handlers

This folder contains handlers for player snapshot endpoints.

## Files

### PlayerHandler.cs

Returns current guild playback/player state for mobile clients.

## Notes

- Player handlers should read snapshots through snapshot services/repositories.
- Playback mutations should go through bot-control command endpoints, not this read handler.
