# Mobile App Repositories

This folder contains mobile app user and session repository implementations.

## Why This Folder Exists

The API turns Discord OAuth login into the app's own session model. This repository folder stores the durable side of that flow: the Discord user, the guilds the user can access, and the refresh sessions used by the mobile app.

## Files

### MobileAppUserRepository.cs

Implements `IMobileAppUserRepository`.

Responsibilities:

- upsert Discord user profile data
- sync a user's accessible guild list
- read a mobile app user
- read guilds available to a user
- check whether a user can access a guild

### MobileAppSessionRepository.cs

Implements `IMobileAppSessionRepository`.

Responsibilities:

- create refresh sessions
- look up sessions by refresh token hash
- rotate refresh token hashes
- revoke sessions

## Flow Context

Login upserts the Discord user and syncs guild links against guilds known by the bot. Authenticated API calls later use those links to list guilds and guard guild-scoped commands. Refresh sessions are rotated by the API so a stolen old refresh token cannot be reused after a successful refresh.

## Maintenance Notes

- The repository stores only guild links for guilds known in `guild_data`.
- Refresh tokens are persisted as hashes only.
- Keep session revocation and rotation behavior explicit; do not silently overwrite session rows without considering device logout behavior.
