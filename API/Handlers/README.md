# Handlers

This folder contains minimal API handler methods grouped by feature.

## Overview

Handlers are the boundary between HTTP endpoints and application behavior.
They should:

- parse route/query/body inputs supplied by minimal API binding
- read authenticated user claims when needed
- call persistence/service contracts
- return `IResult` responses
- use `DomainToHttpMapper` for normal JSON API outcomes

Handlers should not contain EF Core queries directly and should not hold long-running state.

## Feature Folders

- `Auth/` - Discord OAuth callback, app session exchange, refresh, and logout
- `BotControl/` - bot-control command creation and command status lookup
- `Guilds/` - authenticated user guild listing
- `Playback/` - API-to-bot playback command enqueueing
- `Player/` - current player/playback snapshot reads
- `Playlists/` - saved playlist CRUD and track mutation handlers
- `Queue/` - queue snapshot reads and queue command submission
- `Profile/` - authenticated mobile profile and settings reads/writes
- `Status/` - database/API health checks

## Notes

- OAuth redirect responses are intentionally returned directly with `Results.Redirect`/`HttpResults.Redirect`; they are not normal JSON domain results.
- Authenticated handlers should read Discord user ID from JWT claims, not from user-controlled query parameters.
- Playback, queue, and player mutations should enqueue bot-control commands instead of executing Discord/Lavalink operations inside the API process.
