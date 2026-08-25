# Mobile App Persistence Contracts

This folder contains mobile app user and session persistence contracts.

## Why This Folder Exists

The mobile app logs in with Discord, but the API uses its own app session after the OAuth flow. This folder defines the persistence boundary for that flow.

The API needs to store the Discord user, sync the guilds that are both visible to the user and known by the bot, and rotate refresh sessions without exposing token storage details to handlers.

## Files

### IMobileAppUserRepository.cs

Contract for mobile app users and their accessible guild list.

### IMobileAppSessionRepository.cs

Contract for refresh session creation, lookup, rotation, and revocation.

## Flow Context

After Discord login, the API upserts the mobile app user and syncs user-guild links. Later authenticated guild endpoints read the current Discord user ID from the JWT and use these contracts to return only guilds the user can access.

Refresh sessions are separate from users because one user may log in from multiple devices and each device session needs its own revocation/rotation lifecycle.

## What Belongs Here

- mobile app user upsert/read contracts
- user-guild sync and access checks
- refresh session creation, rotation, lookup, and revocation

## What Does Not Belong Here

- Discord OAuth HTTP calls
- JWT token generation
- API request/response DTOs

## Maintenance Notes

- Discord login data is stored behind mobile app user records.
- User-guild access is stored separately from guild data.
- Refresh sessions are stored as hashed tokens.
