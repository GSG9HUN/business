# Guild Repositories

This folder contains guild-level repository implementations.

## Why This Folder Exists

Most persistence operations require a known guild row. This repository centralizes guild bootstrap and premium-state operations so other repositories do not duplicate root-row creation logic.

## Files

### GuildDataRepository.cs

Implements `IGuildDataRepository`.

Responsibilities:

- ensure guild row exists
- query effective premium status
- upsert premium state values

### GuildDataBootstrapper.cs

Internal helper for creating missing guild data rows.

## Flow Context

Queue, playlist, playback, and repeat-list repositories can ensure guild data exists before writing child rows. Premium-sensitive flows can query the effective premium state through this repository.

## Maintenance Notes

- Guild data is the parent row for most guild-scoped persistence.
- Keep guild bootstrap behavior idempotent.
