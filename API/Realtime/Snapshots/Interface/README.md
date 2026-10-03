# Realtime Snapshot Interfaces

This folder contains contracts for realtime snapshot providers.

## Files

### IRealtimeSnapshotProvider.cs

Abstraction for building snapshots attached to realtime events.

## Notes

- Listener and publisher code should depend on this interface instead of manually rebuilding snapshot data.
