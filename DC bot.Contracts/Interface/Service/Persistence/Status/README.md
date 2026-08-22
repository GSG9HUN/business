# Status Persistence Contracts

This folder contains contracts used to report persistence health.

## Why This Folder Exists

Status endpoints need a small, safe way to ask whether the configured database is reachable. They should not open EF Core contexts directly from the API layer, and they should not know how the persistence project checks connectivity.

`IDbStatusCheck` is that boundary.

## Files

### IDbStatusCheck.cs

Defines a single connectivity check:

- returns `true` when the database can be reached
- returns `false` when the repository implementation handles a connection failure
- keeps database provider details inside `DC bot.Persistence`

## Usage Flow

The API status handler depends on this contract. The persistence project implements it with EF Core. This keeps the health endpoint readable while still testing the real configured database path.

## Maintenance Notes

Do not add broad database diagnostics here unless an endpoint or service actually needs them. This contract should stay intentionally small.
