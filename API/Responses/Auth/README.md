# Auth Responses

This folder contains DTOs used by Discord OAuth and mobile app session endpoints.

## Files

### AuthSessionResponse.cs

Returned by app session exchange and refresh endpoints.

Fields:

- `AccessToken` - short-lived app JWT
- `RefreshToken` - opaque refresh token
- `ExpiresInSeconds` - access token lifetime

### DiscordTokenResponse.cs

Discord OAuth token response DTO.

### DiscordUserResponse.cs

Discord current-user response DTO.

### DiscordGuildResponse.cs

Discord current-user guild response DTO.

## Notes

- Discord snowflake IDs are represented as strings in Discord DTOs and parsed into `ulong` before persistence.
- Discord access tokens are server-side only and should not be returned to Android.
