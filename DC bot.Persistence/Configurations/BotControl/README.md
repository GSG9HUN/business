# Bot Control Configurations

This folder contains EF Core mappings for bot control command entities.

## Why This Folder Exists

Bot-control commands are the database bridge between the API process and the bot worker. The mapping here controls how command IDs, guild IDs, user IDs, command type, state, timestamps, and failure information are persisted.

That schema matters because command claiming and state updates must be predictable across two processes.

## Files

### BotControlCommandsConfiguration.cs

Maps bot control command storage.

## What To Keep Here

- table and column names for command storage
- command state conversion if needed
- indexes used by pending-command claim queries
- timestamp and failure-message constraints

## Maintenance Notes

- Command rows are used as the database bridge between the API and bot worker.
- Keep query indexes aligned with `BotControlCommandsRepository`.
