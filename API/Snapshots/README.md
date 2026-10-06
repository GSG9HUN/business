# Snapshots

This folder contains API snapshot services.

## Purpose

Snapshot services assemble read models used by REST endpoints and realtime payloads.

## Subfolders

### Playback/

Playback snapshot assembly for current track, queue count, repeat flags, guild display data, and bot voice status.

## Notes

- Snapshot services should stay read-focused.
- Mutating playback actions should remain command-based through bot-control.
