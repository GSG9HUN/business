# Guild Persistence Contracts

This folder contains guild-level persistence contracts.

## Why This Folder Exists

Almost every persisted bot feature is guild-scoped. Queue items, playback state, repeat-list items, playlists, premium state, and mobile app guild access all rely on a known guild row.

This contract keeps guild bootstrap and premium checks behind one boundary instead of duplicating "create guild row if missing" logic throughout the codebase.

## Files

### IGuildDataRepository.cs

Repository contract for guild row creation and premium state operations.

## How It Is Used

Services call `EnsureGuildExistsAsync` before inserting guild-owned records. Premium-sensitive features call `IsPremiumAsync` to check the effective premium state, including expiry.

## What Belongs Here

- guild row bootstrap operations
- guild premium read/write operations
- contract methods that are truly guild-root concerns

## What Does Not Belong Here

- queue operations
- playback state operations
- playlist operations
- API authorization checks

## Maintenance Notes

- The implementation ensures guild rows exist before related data is inserted.
- Premium state is stored on the guild data row.
