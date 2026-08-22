# Playback Handlers

This folder contains handlers for API-originated playback commands.

## Why This Folder Exists

The mobile API should not execute Discord or Lavalink playback directly. Playback is owned by the bot process. API playback handlers therefore validate HTTP/auth context and persist a bot-control command that the bot worker can execute.

## Files

### PlaybackHandler.cs

Accepts playback command requests and stores them as bot-control commands.

Responsibilities:

- read validated `guildId` from `HttpContext.Items`
- enqueue commands through `IBotControlCommandsRepository`
- return `202 Accepted` with the command ID and state

## Flow Context

A mobile client calls a playback command endpoint for a guild. The endpoint validates `guildId`, the handler should validate the authenticated user context, then it enqueues a command row. The bot process later claims that row and performs the actual pause/resume/skip/stop behavior.

## Maintenance Notes

- Current implementation still has a TODO for reading the authenticated user ID from claims.
- Actual Discord/Lavalink playback execution should remain in the bot worker process, not in API handlers.
