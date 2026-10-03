# Realtime Snapshots

This folder contains snapshot helpers for realtime events.

## Files

### RealtimeSnapshotProvider.cs

Builds REST-equivalent playback, queue, and guild bot status snapshots for realtime event payloads.

## Subfolders

### Interface/

Snapshot provider contracts.

## Notes

- Realtime payloads should not drift from REST read endpoint behavior.
- If a REST snapshot shape changes, update the realtime snapshot provider alongside it.
