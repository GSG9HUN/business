# Auth Services

This folder contains services used by Discord OAuth and mobile session handling.

## Files

### DiscordOAuthService.cs

Builds Discord authorization URLs and calls Discord OAuth/user APIs.

Responsibilities:

- create `/oauth2/authorize` URLs with `identify guilds` scopes
- exchange authorization codes for Discord tokens
- fetch the current Discord user
- fetch current user guilds

### AppTokenService.cs

Creates and hashes app-level tokens.

Responsibilities:

- create 15-minute JWT access tokens
- create opaque refresh tokens
- hash refresh tokens before persistence lookup/storage

### OAuthStateStore.cs

Stores short-lived OAuth `state` values for callback validation.

### AuthTicketStore.cs

Stores short-lived one-time auth tickets used by Android after Discord callback redirect.

## Notes

- Current state and ticket stores use in-memory cache and are suitable for a single API instance.
- Use shared storage such as Redis or database-backed tickets before running multiple API instances.
- Discord tokens should remain server-side only.
