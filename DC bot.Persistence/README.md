# DC bot.Persistence

This project contains all database persistence implementation based on EF Core and PostgreSQL.

## Overview

The persistence layer stores guild-related runtime state and the command bridge records shared by the API and bot processes:

- premium status
- premium audit table/entity schema
- playback state (repeat flags, current track)
- queue items and queue ordering
- repeat-list track identifiers
- saved playlists and ordered playlist tracks
- bot control commands written by the API and consumed by the bot worker

Persistence operations are exposed through interfaces in `../DC bot.Contracts/Interface/Service/Persistence/` and implemented by repositories in this project.

## Subfolders

- `Db/` - `BotDbContext` and design-time context factory
- `Entities/` - EF Core entity classes mapped to tables
- `Configurations/` - EF Core entity mapping configuration classes
- `Repositories/` - repository implementations used by services
- `Migrations/` - EF Core migrations and model snapshot
- `DependencyInjection/` - `AddPersistenceServices(...)` registration for bot and API processes

## Notes

- Discord IDs are represented as `ulong` in domain/service contracts and converted to `long` for database columns.
- `Db/DatabaseMigrationRunner.cs` applies pending migrations automatically at startup.
- PostgreSQL connection settings are read from environment variables.
- `GuildPremiumAuditEntity` is mapped in the EF model, but there is currently no dedicated audit repository.
- Playlist repositories expose immutable record contracts to the service layer; EF entities stay inside this project.
- Both `API` and `DC bot` may reference this project. Neither process should reference the other executable project for database access.

## Related Components

- `../DC bot.Contracts/Interface/Service/Persistence/README.md` - persistence contracts
- `../DC bot/Service/Music/README.md` - business logic that uses repositories
- `../DC bot/PROGRAM_CS_README.md` - bot startup flow and migration step
- `../DC bot/Startup/README.md` - bot startup composition
