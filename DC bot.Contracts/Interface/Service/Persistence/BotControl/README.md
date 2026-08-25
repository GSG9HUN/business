# Bot Control Persistence Contracts

This folder contains contracts for the API-to-bot command bridge.

## Why This Folder Exists

The API and the bot are separate processes. The API cannot directly call Lavalink or Discord bot services inside the bot process. Instead, it writes a command row to the database and the bot worker claims that command.

This contract describes that bridge without exposing the database table or EF Core entity.

## Files

### IBotControlCommandsRepository.cs

Repository contract for queueing and processing bot control commands.

## Flow Context

1. API receives an authenticated mobile app command.
2. API verifies the user can access the guild.
3. API enqueues a bot-control command through this repository contract.
4. Bot worker claims pending commands.
5. Bot worker marks the command done or failed.

## What Belongs Here

- enqueue command contract
- claim next pending command contract
- mark done/failed contracts

## What Does Not Belong Here

- Discord command execution
- Lavalink playback logic
- API authorization logic

## Maintenance Notes

- The API writes commands.
- The bot worker claims commands and marks them done or failed.
