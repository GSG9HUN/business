# Persistence Interface Models

This folder contains immutable record models returned by persistence contracts.

## Files

- `PlaybackStateRecord.cs`
- `QueueItemRecord.cs`

## Purpose

These records decouple service logic from EF Core entities and provide stable, test-friendly data contracts.

## Notes

- `GuildId` is represented as `ulong` at contract level.
- Queue item `State` is numeric and mapped by repository logic.
