# Bot Control Repositories

This folder contains bot control command repository implementations.

## Why This Folder Exists

The mobile/API side needs to request bot actions, but the bot process owns the actual Discord and Lavalink runtime. This repository persists commands so the two processes can communicate through the database.

## Files

### BotControlCommandsRepository.cs

Implements `IBotControlCommandsRepository`.

Responsibilities:

- enqueue bot control commands
- claim pending commands
- mark commands done
- mark commands failed

## Flow Context

The API enqueues a command after authentication and guild-access validation. The bot worker claims pending commands, executes the matching action, and updates the row to done or failed.

## Maintenance Notes

- This repository is the database bridge between the API process and bot worker process.
- Keep claim queries aligned with indexes in `Configurations/BotControl`.
