# Playback Snapshot Interfaces

This folder contains playback snapshot service contracts.

## Files

### IPlaybackSnapshotService.cs

Contract for building playback snapshots used by REST and realtime flows.

## Notes

- Handlers and realtime providers should depend on this interface for playback read state.
