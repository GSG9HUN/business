# Persistence Interface Models

This folder contains immutable record models and enums used by persistence contracts.

## Why This Folder Exists

Repository methods should not return EF Core entities directly. Entities carry database mapping concerns, navigation properties, and persistence behavior that service code should not depend on.

The records in this folder are the safer boundary shape:

- immutable by default
- easy to assert in tests
- independent from EF Core table configuration
- stable for bot/API service code

They also keep API response DTOs separate from persistence results. A handler can map a persistence record into an API response without exposing database naming or internal columns to clients.

## Subfolders

### Playback

Playback state records.

### Queue

Queue item records and queue state enum.

### Playlists

Saved playlist metadata and track records.

### MobileApps

Mobile app user, user-guild, and refresh session records.

### BotControlCommand

Bot command bridge records and command state enum.

## How To Choose The Right Model

Use these records when data crosses the persistence contract boundary. Do not use EF entities outside `DC bot.Persistence`, and do not reuse API response DTOs as repository return types.

If a method needs input data for persistence and that input is not an HTTP request payload, create a small contract input record here. `MobileAppUserUpsertRecord` is an example of that pattern.

## Maintenance Notes

- `GuildId` and Discord user IDs are represented as `ulong` at contract level.
- Queue item `State` uses the explicit `QueueItemState` enum at contract level.
- Bot control command state uses `BotControlCommandState` for the API-to-bot command bridge.
- EF Core entities stay in `DC bot.Persistence/Entities`.
