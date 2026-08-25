# Endpoints

This folder contains minimal API route registration extensions.

## Overview

Endpoint classes define the public HTTP route surface and connect routes to handler methods.
They should stay thin:

- create route groups
- attach validation filters
- attach authorization requirements
- map HTTP verbs to handlers

Business logic belongs in `Handlers/`, service orchestration belongs in services, and persistence stays behind repository contracts.

## Files

### AuthEndpoints.cs

Maps `/api/auth` routes.

Routes:

- `POST /api/auth/discord/start` - create Discord authorization URL
- `GET /api/auth/discord/callback` - receive Discord OAuth callback
- `POST /api/auth/exchange` - exchange mobile auth ticket for app session tokens
- `POST /api/auth/refresh` - rotate refresh token and issue new access token
- `POST /api/auth/logout` - revoke a refresh session

### GuildEndpoints.cs

Maps authenticated `/api/guilds` routes.

Responsibilities:

- require authorization for guild access endpoints
- expose the current user's accessible guild list

### ProfileEndpoints.cs

Maps authenticated `/api/profile` routes.

Routes:

- `GET /api/profile` - return the authenticated mobile user's profile and settings
- `PATCH /api/profile/settings` - update selected profile settings fields

### PlaybackEndpoints.cs

Maps `/api/guilds/{guildId}/playback` command endpoints.

Responsibilities:

- validate `guildId`
- validate playback command names
- enqueue bot-control commands through the playback handler

### PlayerEndpoints.cs

Reserved for player snapshot/status routes.

### PlaylistEndpoints.cs

Reserved for playlist CRUD and playback routes.

### QueueEndpoints.cs

Reserved for queue read/write routes.

### StatusEndpoints.cs

Maps `/api/status` health/status routes.

## Notes

- Keep endpoint files focused on routing only.
- Return the original parent `RouteGroupBuilder` from extension methods so endpoint registration can be chained safely.
- Use feature-specific handlers from `API.Handlers.*` namespaces.
