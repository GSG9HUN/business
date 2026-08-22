# Responses

This folder contains response DTOs grouped by feature.

## Why This Folder Exists

Response DTOs define the JSON shape returned to mobile/API clients. They are the public output contract of the API.

They should be stable, explicit, and decoupled from EF Core entities. This allows persistence tables and internal contract records to evolve without accidentally changing what the mobile app receives.

## Feature Folders

- `Auth/` - Discord OAuth DTOs and mobile session responses
- `Guilds/` - guild summary and status responses
- `Playback/` - playback status responses
- `Playlists/` - playlist summary/detail/track responses
- `Queue/` - queue and queue-track responses

## What Belongs Here

- DTOs returned by API endpoints
- Discord OAuth transport DTOs used by the auth service
- app-facing response shapes grouped by feature

## What Does Not Belong Here

- request DTOs
- EF Core entities
- repository contract records
- bot worker command entities

## Maintenance Notes

- Do not return EF entities directly from API handlers.
- Keep Discord API transport DTOs in `Auth/` because they are part of the login/session boundary.
