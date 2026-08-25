# Queue Persistence Contracts

This folder contains queue-related persistence contracts.

## Why This Folder Exists

The queue is shared runtime state. Commands enqueue tracks, playback services claim the next track, and queue commands reorder or clear the remaining items.

This contract makes those operations explicit and keeps concurrency-sensitive queue behavior behind one repository boundary.

## Files

### IQueueRepository.cs

Contract for queue item lifecycle operations.

Responsibilities:

- read queued items
- enqueue one or many tracks
- reorder queued tracks
- update queue item position
- mark items as playing, played, or skipped
- atomically claim the next queued item

## Why Claiming Is Separate

`ClaimNextQueuedItemAsync` is different from simply reading the next item. It must mark one item as playing as part of the same operation so two workers cannot start the same track.

The implementation uses database-specific locking, but callers only depend on this contract.

## What Belongs Here

- queue reads
- enqueue operations
- queue reordering
- queue item state transitions
- atomic queue claim operations

## What Does Not Belong Here

- Lavalink playback calls
- playlist storage
- Discord command parsing

## Maintenance Notes

- Queue state is represented by `QueueItemState` at contract level.
- Implementations should enforce queue ordering consistently.
