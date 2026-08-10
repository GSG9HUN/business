# Playlist Models

This folder contains saved playlist persistence records.

## Why This Folder Exists

Playlist services need data for two different views: playlist metadata and ordered playlist tracks. These records represent those views without exposing EF Core navigation properties or database table details.

## Files

### PlaylistRecord.cs

Immutable saved playlist detail record.

### PlaylistSummaryRecord.cs

Immutable saved playlist summary record with track count.

### PlaylistTrackCreateRecord.cs

Input record used when adding tracks to a playlist.

### PlaylistTrackRecord.cs

Immutable saved playlist track record.

## Flow Context

`PlaylistSummaryRecord` supports list views without loading every track. `PlaylistRecord` and `PlaylistTrackRecord` support detail/load flows where ordered tracks are needed. `PlaylistTrackCreateRecord` is used when commands ask the repository to append tracks.

## Maintenance Notes

- Playlist models are used by playlist services instead of EF Core entities.
- Keep playlist ordering explicit through order numbers.
- Do not add API-only formatting fields here.
