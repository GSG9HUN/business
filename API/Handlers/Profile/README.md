# Profile Handlers

This folder contains handlers for authenticated mobile profile and settings endpoints.

## Why This Folder Exists

Profile endpoints are user-scoped. They should read the Discord user ID from the app JWT and never accept a user ID from the request body or query string.

The handler combines mobile user profile data with persisted app settings so the Android profile screen can load a stable response shape.

## Files

### ProfileHandler.cs

Handles profile and settings requests.

Responsibilities:

- read Discord user ID from the JWT `sub` claim
- load the mobile app user from persistence
- load or create default settings
- merge partial settings updates
- map persistence records into API response DTOs

## Maintenance Notes

- Keep settings defaults in persistence/repository code.
- Keep request bodies user-id free; authenticated identity comes from JWT claims.
- Add validation before accepting new language or theme values from the client.
