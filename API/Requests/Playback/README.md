# Playback Requests

This folder contains request DTOs for playback endpoints.

## Files

### SetRepeatModeRequest.cs

Request body for explicitly setting repeat mode.

`mode` values:

- `none` - disable repeat modes
- `one` - repeat the current track
- `all` - repeat the active queue/list snapshot

## Notes

- Request DTOs should stay transport-only.
- Unlike command-submission endpoints, `PlaybackHandlers.SetRepeatModeAsync` updates persisted playback state and the repeat-list snapshot directly and returns `204 No Content`; it does not enqueue a bot-control command.
