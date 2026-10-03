# Queue Handlers

This folder contains handlers for queue read and write endpoints.

## Files

### QueueHandlers.cs

Handles queue snapshot reads and command submission for enqueue, clear, remove, shuffle, and move operations.

## Notes

- Queue read operations return persisted queue snapshots.
- Queue write operations enqueue bot-control commands so the bot process performs Discord/Lavalink work.
- Keep queue numbering rules aligned with endpoint docs: remove uses 1-based track numbers, move uses 0-based indexes.
