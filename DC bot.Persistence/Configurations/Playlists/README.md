# Playlist Configurations

This folder contains EF Core mappings for playlist entities.

## Why This Folder Exists

Saved playlists have two related table shapes: playlist metadata and ordered playlist tracks. The mappings here enforce the rules that make playlist operations deterministic, such as unique playlist names per guild and unique order numbers within one playlist.

## Files

### PlaylistConfiguration.cs

Maps saved playlist metadata.

### PlaylistTrackConfiguration.cs

Maps ordered playlist tracks.

## What To Keep Here

- playlist table and column names
- playlist name uniqueness per guild
- playlist track table mapping
- track ordering uniqueness per playlist
- cascade behavior between playlists and tracks

## Maintenance Notes

- Playlist names are unique per guild.
- Track order numbers are unique inside one playlist.
