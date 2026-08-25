# Mobile App Models

This folder contains mobile app user and session persistence records.

## Why This Folder Exists

The mobile app auth flow has data that is not the same as an API request and not the same as an EF entity. These records describe what crosses the repository boundary during Discord login, guild sync, and refresh-session handling.

## Files

### MobileAppUserRecord.cs

Immutable mobile app user record.

### MobileAppUserUpsertRecord.cs

Input record for inserting or updating a mobile app user.

### MobileAppUserGuildRecord.cs

Immutable record describing one guild visible to a mobile app user.

### MobileAppUserGuildUpsertRecord.cs

Input record for syncing a user's visible guild list.

### MobileAppSessionRecord.cs

Immutable refresh session record.

## Flow Context

During login, Discord user data is converted into `MobileAppUserUpsertRecord`. Guild data from Discord is converted into `MobileAppUserGuildUpsertRecord`, then the repository stores only guild links that match guilds known by the bot.

When the app refreshes its session, the API uses `MobileAppSessionRecord` to validate expiry/revocation and rotate the stored refresh token hash.

## Maintenance Notes

- These models represent Discord login/session data at the persistence contract boundary.
- Store token hashes and session metadata here, not raw refresh token values.
- Keep API response formatting in `API/Responses/Auth`.
