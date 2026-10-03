# Realtime Events

This folder contains SignalR payload records emitted to mobile clients.

## Files

### PlaybackSnapshotEvent.cs

Guild-scoped playback event payload with the current playback snapshot.

### QueueSnapshotEvent.cs

Guild-scoped queue event payload with the current queue snapshot.

### GuildBotStatusEvent.cs

Guild-scoped bot status event payload.

### BotControlCommandEvent.cs

User/guild-scoped command state payload for API-originated bot-control commands.

## Notes

- Event names should match the bot/mobile realtime event constants.
- Payload snapshots should stay compatible with REST response shapes where practical.
