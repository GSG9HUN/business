# Persistence Repositories

This folder contains repository implementations for persistence contracts.

## Why This Folder Exists

Repositories are the only normal application code that should know both sides of the persistence boundary:

- contract interfaces and records from `DC bot.Contracts`
- EF Core context, entities, and mapping behavior from this project

That makes repositories the translation layer between service-friendly methods and database operations.

## Subfolders

### Guilds

Guild data and premium state repository implementation.

### Playback

Playback state and repeat-list repository implementations.

### Queue

Queue repository implementation and queue claim helpers.

### Playlists

Saved playlist and playlist track repository implementations.

### MobileApps

Mobile app user, user-guild access, and refresh session repository implementations.

### BotControl

API-to-bot command bridge repository implementation.

### Status

Database status repository implementation.

## How Repositories Should Behave

Repository methods should express use cases rather than raw table access. For example, `ClaimNextQueuedItemAsync` is better than exposing a generic "update queue row" method because the operation has concurrency rules that belong inside the repository implementation.

Use short-lived contexts from `IDbContextFactory<BotDbContext>`. Keep transactions close to the operation that needs atomic behavior.

## Maintenance Notes

- Repositories use `IDbContextFactory<BotDbContext>` and short-lived contexts.
- Guild IDs are represented as `ulong` in repository contracts and EF entities.
- Queue item state is represented as `QueueItemState` in repository code and converted to the `short` database value by EF configuration.
- Repository namespaces remain stable even though files are feature-scoped physically.
