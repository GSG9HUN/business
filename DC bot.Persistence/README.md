# DC bot.Persistence

This project contains all database persistence implementation based on EF Core and PostgreSQL.

## Why This Project Exists

The bot and API both need durable state, but neither executable project should own database mapping details. This project is the single place where PostgreSQL tables, EF Core entities, migrations, and repository implementations live.

The rest of the solution talks to this project through contracts from `../DC bot.Contracts/Interface/Service/Persistence/`.

That gives the codebase a clear boundary:

- services ask repositories for domain-friendly data
- repositories translate between contract records and EF entities
- EF configuration describes how entities map to PostgreSQL
- migrations describe how schema changes are applied

The persistence layer stores guild-related runtime state and the command bridge records shared by the API and bot processes:

- premium status
- premium audit table/entity schema
- playback state (repeat flags, current track)
- queue items and queue ordering
- repeat-list track identifiers
- saved playlists and ordered playlist tracks
- bot control commands written by the API and consumed by the bot worker
- mobile app users, user-guild access, and refresh sessions

## Subfolders

- `Db/` - `BotDbContext` and design-time context factory
- `Entities/` - EF Core entity classes mapped to tables, grouped by feature
- `Configurations/` - EF Core entity mapping configuration classes, grouped by feature
- `Repositories/` - repository implementations used by services, grouped by feature
- `Migrations/` - EF Core migrations and model snapshot
- `DependencyInjection/` - `AddPersistenceServices(...)` registration for bot and API processes

## How A Feature Is Usually Added

Most persisted features touch the same set of places:

1. Add or update a contract in `DC bot.Contracts`.
2. Add or update an EF entity in `Entities/<Feature>/`.
3. Add or update EF mapping in `Configurations/<Feature>/`.
4. Add or update repository implementation in `Repositories/<Feature>/`.
5. Register the repository in `DependencyInjection/`.
6. Add an EF migration.

This keeps contract shape, database shape, and repository behavior aligned.

## Maintenance Notes

- Discord IDs are represented as `ulong` in domain/service contracts and converted to `long` for database columns.
- `Db/DatabaseMigrationRunner.cs` applies pending migrations automatically at startup.
- PostgreSQL connection settings are read from environment variables.
- `GuildPremiumAuditEntity` is mapped in the EF model, but there is currently no dedicated audit repository.
- Playlist repositories expose immutable record contracts to the service layer; EF entities stay inside this project.
- Mobile app refresh tokens are persisted as hashes, not raw token values.
- Both `API` and `DC bot` may reference this project. Neither process should reference the other executable project for database access.
- Physical folders are feature-scoped, while namespaces remain stable for existing consumers.

## Related Components

- `../DC bot.Contracts/Interface/Service/Persistence/README.md` - persistence contracts
- `../DC bot/Service/Music/README.md` - business logic that uses repositories
- `../DC bot/PROGRAM_CS_README.md` - bot startup flow and migration step
- `../DC bot/Startup/README.md` - bot startup composition
