# Bot Control Responses

This folder contains response DTOs for API-originated bot-control commands.

## Files

### BotControlCommandResponse.cs

Returned when a command is accepted/enqueued.

### BotControlCommandStatusResponse.cs

Returned when a client asks for command execution state.

## Notes

- These DTOs describe command acceptance/status, not direct playback state.
- Playback and queue snapshots are returned by their feature-specific response DTOs.
