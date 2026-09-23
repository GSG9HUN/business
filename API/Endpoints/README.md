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
- `GET /api/profile/me` - return only the authenticated mobile user's Discord profile fields
- `PATCH /api/profile/settings` - update selected profile settings fields

`GET /api/profile/me` response:

```json
{
  "discordUserId": "123456789012345678",
  "username": "example_user",
  "displayName": "Example User",
  "avatarUrl": "https://cdn.discordapp.com/avatars/123456789012345678/avatarhash.webp?size=128",
  "isDiscordConnected": true,
  "isActive": true
}
```

### PlaybackEndpoints.cs

Maps `/api/guilds/{guildId}/playback` command endpoints.

Responsibilities:

- validate `guildId`
- validate playback command names
- enqueue bot-control commands through the playback handler

Routes:

- `POST /api/guilds/{guildId}/playback/{commandName}` - enqueue a playback command
- `PATCH /api/guilds/{guildId}/playback/repeat-mode` - set repeat mode explicitly

Allowed playback command names:

- `pause`
- `resume`
- `skip`
- `previous`
- `leave`
- `stop`
- `repeat`
- `repeatList`

`PATCH /api/guilds/{guildId}/playback/repeat-mode` request:

```json
{
  "mode": "all"
}
```

`mode` must be one of:

- `none` - disable single-track repeat and queue repeat, then clear the repeat-list snapshot
- `one` - enable single-track repeat, disable queue repeat, then clear the repeat-list snapshot
- `all` - disable single-track repeat, enable queue repeat, then replace the repeat-list snapshot with the current track plus queued tracks

Successful repeat-mode updates return `204 No Content`.

### PlayerEndpoints.cs

Maps authenticated `/api/guilds/{guildId}/player` snapshot routes.

Routes:

- `GET /api/guilds/{guildId}/player` - return the current playback snapshot for the guild

`GET /api/guilds/{guildId}/player` response:

```json
{
  "guildId": "123456789012345678",
  "guildName": "Example Guild",
  "guildIconUrl": "https://cdn.discordapp.com/icons/123456789012345678/iconhash.webp?size=128",
  "botStatus": {
    "isOnline": true,
    "connectedVoiceChannelName": "Music",
    "connectedVoiceUserCount": 4
  },
  "currentTrack": {
    "title": "Track title",
    "author": "Track author",
    "duration": 180,
    "trackUri": "https://example.com/track",
    "artworkUri": "https://example.com/artwork.jpg",
    "requestedBy": "Example User"
  },
  "isPlaying": true,
  "isPaused": false,
  "positionSeconds": 42,
  "queueTrackCount": 2,
  "isRepeating": false,
  "isRepeatingList": true,
  "updatedAtUtc": "2026-09-22T07:15:30Z"
}
```

Notes:

- `currentTrack` is `null` when nothing is currently playing.
- `guildIconUrl`, `connectedVoiceChannelName`, `artworkUri`, and `requestedBy` may be `null`.
- `duration` and `positionSeconds` are expressed in seconds.

### PlaylistEndpoints.cs

Reserved for playlist CRUD and playback routes.

### QueueEndpoints.cs

Maps authenticated `/api/guilds/{guildId}/queue` read/write routes.

Routes:

- `GET /api/guilds/{guildId}/queue` - return the current queued tracks
- `POST /api/guilds/{guildId}/queue/enqueue` - enqueue a play command for a query or URL
- `DELETE /api/guilds/{guildId}/queue` - enqueue a clear-queue command
- `DELETE /api/guilds/{guildId}/queue/{trackNumber}` - enqueue a remove-track command by 1-based queue number
- `POST /api/guilds/{guildId}/queue/shuffle` - enqueue a shuffle command
- `PATCH /api/guilds/{guildId}/queue/{trackIndex}/move-up` - enqueue a move-up command by 0-based queue index
- `PATCH /api/guilds/{guildId}/queue/{trackIndex}/move-down` - enqueue a move-down command by 0-based queue index

`GET /api/guilds/{guildId}/queue` response:

```json
{
  "guildId": "123456789012345678",
  "trackCount": 2,
  "tracks": [
    {
      "position": 1,
      "title": "First queued track",
      "author": "Track author",
      "duration": 180,
      "trackUri": "https://example.com/track-1",
      "artworkUri": "https://example.com/artwork-1.jpg",
      "requestedBy": "Example User"
    },
    {
      "position": 2,
      "title": "Second queued track",
      "author": "Track author",
      "duration": 210,
      "trackUri": "https://example.com/track-2",
      "artworkUri": null,
      "requestedBy": null
    }
  ]
}
```

`POST /api/guilds/{guildId}/queue/enqueue` request:

```json
{
  "query": "https://example.com/track",
  "searchMode": null
}
```

Notes:

- `position` is 1-based in queue responses.
- `trackNumber` is 1-based for remove requests.
- `trackIndex` is 0-based for move requests.
- `duration` is expressed in seconds.
- `artworkUri` and `requestedBy` may be `null`.
- Queue write routes enqueue bot-control commands and return `202 Accepted` with command data.

### StatusEndpoints.cs

Maps `/api/status` health/status routes.

## Notes

- Keep endpoint files focused on routing only.
- Return the original parent `RouteGroupBuilder` from extension methods so endpoint registration can be chained safely.
- Use feature-specific handlers from `API.Handlers.*` namespaces.
