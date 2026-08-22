# Queue Models

This folder contains queue persistence records and enums.

## Why This Folder Exists

Queue data moves between commands, music services, and repositories. The service layer needs queue item state and ordering information, but it should not receive EF entities.

These models describe persisted queue data in a database-independent way.

## Files

### QueueItemRecord.cs

Immutable queue item projection returned by queue repository methods.

### QueueItemState.cs

Contract-level queue item state enum.

## Flow Context

Queue commands use `QueueItemRecord` to display queued tracks. Playback services use it when claiming or moving through queue items. `QueueItemState` keeps state transitions readable in code instead of passing raw numeric database values.

## Maintenance Notes

- Queue item state is exposed as an enum instead of raw database values.
- Keep enum values stable because they represent persisted queue states.
