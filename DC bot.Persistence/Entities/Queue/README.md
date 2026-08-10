# Queue Entities

This folder contains queue-related EF Core entities.

## Why This Folder Exists

The queue table is the durable representation of tracks waiting to be played or already processed. The entity here is used by EF Core to track queue rows and by repositories to update queue state.

## Files

### GuildQueueItemEntity.cs

Stores one queued track for one guild.

## What The Entity Represents

Each row belongs to one guild and contains the track identifier, order number, state, and timestamps needed by queue and playback services.

## Maintenance Notes

- Queue item state is represented by the contract-level `QueueItemState` enum in code.
- EF configuration maps the state value to the database column.
