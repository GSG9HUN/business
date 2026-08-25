# DC bot.Contracts

This project is the shared contract boundary between the executable projects and the infrastructure projects.

## Why This Project Exists

The bot, the API, and the tests all need to talk about the same persistence operations without knowing how those operations are implemented. This project holds that shared language.

For example, the API can ask for guilds visible to a mobile app user through `IMobileAppUserRepository`, and the bot can claim bot-control commands through `IBotControlCommandsRepository`. Neither caller needs to reference EF Core entities, database mappings, or repository implementation classes.

That separation matters because it keeps dependencies one-way:

- `DC bot` depends on contracts and persistence registrations
- `API` depends on contracts and persistence registrations
- `DC bot.Persistence` implements the contracts
- contracts do not depend on API, bot runtime, Discord clients, or EF Core

## What Belongs Here

Use this project for types that describe a boundary between projects:

- repository interfaces
- immutable records returned by repository interfaces
- small enums used by those records
- input records used by repository methods when they are not API request DTOs

The important rule is that a contract type should still make sense if the storage implementation changes from EF Core/PostgreSQL to another database.

## What Does Not Belong Here

Do not put these here:

- EF Core entities or configuration classes
- ASP.NET Core request/response DTOs
- Discord client wrappers
- concrete service implementations
- business orchestration code

Those belong in the project that owns the behavior.

## Folder Structure

### Interface/Service/Persistence

This is the current home of repository contracts. The path means: service-layer code depends on these persistence abstractions.

The structure is intentionally explicit, but it is also deeper than strictly necessary while this project only contains persistence contracts. A flatter `Persistence/` root folder would also be valid if the project stays focused on repository contracts only.

### Interface/Service/Persistence/Models

Contains immutable records and enums used by persistence interfaces.

These are not EF entities. They are stable data shapes that service code can consume without pulling database-specific behavior into the service layer.

## Maintenance Rules

- Keep public contracts small and focused on use cases.
- Prefer records for returned data because they are immutable and test-friendly.
- Keep database table shape inside `DC bot.Persistence`.
- Keep HTTP payload shape inside `API`.
- When a repository method changes, update the matching README in this project and the implementation README in `DC bot.Persistence`.
