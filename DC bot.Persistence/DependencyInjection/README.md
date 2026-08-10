# Persistence Dependency Injection

This folder contains service registration helpers for the persistence project.

## Why This Folder Exists

Both the bot and API need the same persistence registrations. Centralizing them here prevents the executable projects from duplicating repository wiring and accidentally registering different implementations.

The extension method is also the public composition boundary for this project: callers provide a PostgreSQL connection string, and this project registers the EF context factory plus repository implementations.

## Files

### PersistenceServiceCollectionExtensions.cs

Registers the EF Core context factory and repository implementations.

Responsibilities:

- register `BotDbContext` factory with PostgreSQL provider
- map persistence interfaces to repository implementations
- provide one shared registration method for the bot and API processes

## Notes

- Keep DI wiring centralized here.
- Do not register API-only or bot-only services in this project.
- Add new repository registrations here when a new persistence contract gets an implementation.
