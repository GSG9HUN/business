# Persistence

This folder contains database persistence implementation based on EF Core and PostgreSQL.

## Overview

The persistence layer stores guild-related runtime state:

- premium status and premium audit history
- playback state (repeat flags, current track)
- queue items and queue ordering
- repeat-list track identifiers

All persistence operations are exposed through interfaces in `Interface/Service/Persistence/` and implemented via repositories in this folder.

## Subfolders

- `Db/` - `BotDbContext` and design-time context factory
- `Entities/` - EF Core entity classes mapped to tables
- `Configurations/` - EF Core entity mapping configuration classes
- `Repositories/` - repository implementations used by services
- `Migrations/` - EF Core migrations and model snapshot

## Notes

- Discord IDs are represented as `ulong` in domain/service contracts and converted to `long` for database columns.
- `Program.cs` applies pending migrations automatically at startup.
- PostgreSQL connection settings are read from environment variables.

## Related Components

- `Interface/Service/Persistence/README.md` - persistence contracts
- `Service/Music/README.md` - business logic that uses repositories
- `PROGRAM_CS_README.md` - startup flow and migration step
