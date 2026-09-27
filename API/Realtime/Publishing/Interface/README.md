# Realtime Publishing Interfaces

This folder contains contracts for SignalR realtime publishers.

## Files

### IGuildRealtimePublisher.cs

Publishes guild-scoped playback, queue, and guild bot status updates.

### IBotControlRealtimePublisher.cs

Publishes bot-control command state updates.

## Notes

- Keep listener code dependent on these interfaces rather than concrete publisher implementations.
