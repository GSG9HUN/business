# Guild Responses

This folder contains response DTOs for guild-oriented endpoints.

## Why This Folder Exists

Guild endpoints expose the guilds a mobile app user can access. The API should not return raw Discord OAuth guild payloads or persistence join rows directly because the mobile client needs a stable app-specific shape.

## Files

### GuildSummaryResponse.cs

Represents a compact guild listing item.

### GuildBotStatusResponse.cs

Represents the bot status summary embedded in each guild listing item.

## Flow Context

The guild list endpoint reads the authenticated user's Discord ID from the JWT, queries synced user-guild links, and maps the result into these response DTOs.

## Maintenance Notes

- Mobile guild lists should represent the intersection of user guilds and bot-known guilds.
- Guild-specific response DTOs should not expose internal persistence state unless the mobile app needs it.
