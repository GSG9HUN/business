# Guild Handlers

This folder contains handlers for guild access and guild listing.

## Why This Folder Exists

Guild endpoints are user-scoped. The API must return only guilds that belong to the authenticated Discord user and are also known by the bot.

This handler keeps that rule close to the endpoint and prevents clients from supplying arbitrary Discord user IDs.

## Files

### GuildHandler.cs

Returns the authenticated mobile user's accessible guilds.

Responsibilities:

- read Discord user ID from the JWT `sub` claim
- validate that the claim is a non-zero Discord snowflake
- query `IMobileAppUserRepository`
- return mapped API results

## Flow Context

Discord login syncs user-guild links into persistence. After the app receives its own JWT, guild endpoints read the user ID from that JWT and return the synced guild list for that user.

## Maintenance Notes

- Do not accept `discordUserId` from query parameters on authenticated endpoints.
- Future guild-specific handlers should call `HasGuildAccessAsync` or a shared authorization guard before reading or mutating guild data.
