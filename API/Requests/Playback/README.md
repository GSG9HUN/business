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
- Repeat behavior is executed by the bot process through bot-control/music services.
