# Auth Handlers

This folder contains HTTP handlers for Discord login and mobile app sessions.

## Files

### AuthHandler.cs

Handles the Discord OAuth and app-session flow.

Responsibilities:

- create Discord authorization URLs
- validate OAuth state during callback
- exchange Discord authorization codes for Discord access tokens
- fetch Discord user and guild information
- upsert mobile app users
- sync user-guild access rows
- create short-lived auth tickets for Android deep-link exchange
- exchange tickets for app access/refresh tokens
- refresh app sessions by rotating refresh tokens
- revoke sessions on logout

## Flow

1. Android calls `POST /api/auth/discord/start`.
2. API returns a Discord authorization URL.
3. Discord redirects to `/api/auth/discord/callback`.
4. API syncs user/guild data and redirects to Android with a one-time ticket.
5. Android calls `/api/auth/exchange` with the ticket.
6. API returns a 15-minute access token and refresh token.
7. Android calls `/api/auth/refresh` before access token expiry.

## Notes

- Refresh tokens must be stored only as hashes.
- Auth tickets are one-time use and short-lived.
- The Discord access token is not returned to Android.
