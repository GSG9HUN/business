# Realtime Publishing

This folder contains SignalR publishing services.

## Files

### GuildRealtimePublisher.cs

Publishes guild-scoped playback, queue, and guild bot status events to authorized subscribed clients.

### BotControlRealtimePublisher.cs

Publishes bot-control command state changes to the relevant user/guild realtime groups.

## Subfolders

### Interface/

Publisher contracts used by listeners and handlers.

## Notes

- Publishers should enforce group targeting rules and avoid leaking guild events to unauthorized clients.
- Event payloads should be built from snapshot providers or normalized command state.
