# Mobile App Configurations

This folder contains EF Core mappings for mobile app entities.

## Why This Folder Exists

The mobile app auth flow stores Discord users, guild access links, and refresh sessions. Those tables need explicit constraints because they are security-sensitive: users must map to stable Discord IDs, guild access must point to known guilds, and refresh sessions must store hashes rather than raw tokens.

## Files

### MobileAppUsersConfiguration.cs

Maps mobile app users.

### UserGuildsConfiguration.cs

Maps the user-guild join table.

### MobileAppSessionsConfiguration.cs

Maps refresh sessions.

## What To Keep Here

- mobile app user primary key and profile columns
- user-guild composite key and guild/user relationships
- indexes used for guild access checks
- refresh session primary key, token hash lookup, expiry, and revocation columns

## Maintenance Notes

- `mobile_app_users.discord_user_id` is the primary key.
- `user_guilds` connects Discord users to known guild rows.
- `mobile_app_sessions` stores refresh token hashes and revocation metadata.
