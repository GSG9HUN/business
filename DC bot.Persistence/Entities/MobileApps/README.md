# Mobile App Entities

This folder contains mobile app authentication and access EF Core entities.

## Why This Folder Exists

Discord login gives the API external identity data, but the app needs its own persisted users, guild access links, and refresh sessions. These entities represent that mobile app auth data in the database.

## Files

### MobileAppUserEntity.cs

Stores Discord user profile data used by the mobile app.

### UserGuildEntity.cs

Join entity linking a Discord user to guilds that exist in `guild_data`.

### MobileAppSessionEntity.cs

Stores refresh session metadata and refresh token hashes.

## What The Entities Represent

`MobileAppUserEntity` stores the Discord profile fields needed by the app. `UserGuildEntity` is the join row that says which known guilds the user can access. `MobileAppSessionEntity` represents one refresh session for one device/login.

## Maintenance Notes

- `DiscordUserId` is the primary identifier for mobile app users.
- `UserGuildEntity` represents access, not a full user list on the guild.
- Refresh tokens are stored as hashes only.
