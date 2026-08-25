# Persistence Service Interfaces

This folder contains persistence contracts used by the bot and API service layers.

## Why This Folder Exists

Persistence is shared by both running processes:

- the Discord bot reads and mutates queue, playback, playlist, and command state
- the API reads mobile app access and writes bot-control commands
- tests need to replace persistence with predictable fakes

This folder defines the repository contracts that make those interactions possible without leaking database implementation details into the bot or API.

Implementations live in `../../../../DC bot.Persistence/Repositories/`.

## How The Boundary Works

Callers depend on interfaces from this folder. They receive immutable model records from `Models/`. The persistence project translates between those records and EF Core entities.

That gives the codebase three separate shapes:

- API DTOs describe HTTP payloads
- contract records describe cross-project data
- EF entities describe database tables

Keeping those shapes separate avoids accidental coupling. A database column rename should not force API clients to change, and an API response change should not require entity changes.

## Subfolders

### Guilds

Guild row creation and premium state contracts. Other persistence areas depend on guild rows existing before child records are inserted.

### Playback

Playback state and repeat-list persistence contracts. These are used by the music services to resume and control guild playback behavior.

### Queue

Queue item lifecycle, ordering, state transitions, and atomic queue claim contracts. Queue claim operations are important because the bot worker must not start the same queued item twice.

### Playlists

Saved playlist metadata and playlist track mutation contracts. Playlist metadata and ordered tracks are split because the operations and constraints are different.

### MobileApps

Mobile app user, user-guild access, and refresh session contracts. These support the Discord login to app-session flow.

### BotControl

API-to-bot command bridge contracts. The API writes commands here; the bot process claims and executes them.

### Status

Database connectivity/status contracts used by health/status endpoints.

### Models

Immutable record models and enums used in persistence contract method signatures.

## What Belongs Here

- repository interfaces
- contract records used by those interfaces
- enums needed to describe repository state

## What Does Not Belong Here

- EF Core configuration
- SQL or migrations
- ASP.NET Core endpoint responses
- Discord client types

## Maintenance Notes

- Contracts use `ulong` guild/user identifiers to match Discord domain objects.
- Repository methods should return contract records, not EF Core entities.
- Keep this layer free of database provider and ASP.NET Core dependencies.
- Physical folders are feature-scoped, while namespaces remain stable for existing consumers.
