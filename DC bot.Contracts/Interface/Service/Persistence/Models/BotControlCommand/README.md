# Bot Control Command Models

This folder contains API-to-bot command bridge records.

## Why This Folder Exists

Bot-control commands are persisted so the API can ask the bot process to perform work without directly referencing bot runtime services.

These models describe the command as it crosses the repository boundary.

## Files

### BotControlCommandRecord.cs

Immutable command record returned by command repository operations.

### BotControlCommandState.cs

Contract-level command state enum.

## Flow Context

The API creates a pending command. The bot worker claims pending commands, executes the corresponding bot behavior, then marks the command done or failed. `BotControlCommandState` makes that lifecycle explicit.

## Maintenance Notes

- Command state is shared by the API process and bot worker process.
- Keep state transitions compatible with repository claim/update behavior.
