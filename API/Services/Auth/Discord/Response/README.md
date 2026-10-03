# Discord Auth Response DTOs

This folder contains DTOs for Discord OAuth and user/guild API responses.

## Files

### DiscordTokenResponse.cs

Discord OAuth token exchange response.

### DiscordUserResponse.cs

Discord current-user response used to create/update mobile app users.

### DiscordGuildResponse.cs

Discord guild response used to sync accessible guilds.

## Notes

- These records model Discord's external API shape.
- Do not return these DTOs directly to MobileApp clients; map them to app-facing responses first.
