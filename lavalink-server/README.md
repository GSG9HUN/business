# Lavalink Server Usage Guide

This folder contains the Lavalink runtime configuration used by the Docker Compose stack.

## Runtime

The normal repository workflow starts Lavalink through the root `docker-compose.yaml` file:

```sh
docker compose up -d --build
```

The compose stack mounts this file into the container:

```text
lavalink-server/application.yaml -> /opt/Lavalink/application.yaml
```

Manual `Lavalink.jar` usage is still possible, but Docker Compose is the supported project workflow.

## Docker Compose Networking

Lavalink listens on port `2333` inside the Docker network and is reachable by the bot at:

```text
lavalink:2333
```

The host port is bound to localhost only:

```text
127.0.0.1:2333:2333
```

Do not expose Lavalink publicly. Use SSH port forwarding when remote debugging is needed:

```sh
ssh -N -L 2334:127.0.0.1:2333 ubuntu@<server-ip>
```

## Required Secrets

The Lavalink password is provided by the root `.env` file and must match the bot configuration:

```env
LAVALINK_PASSWORD=your_lavalink_password
```

Docker Compose passes it to Lavalink as:

```text
LAVALINK_SERVER_PASSWORD
```

Optional provider credentials are also read from environment variables:

- `SPOTIFY_CLIENT_ID`
- `SPOTIFY_CLIENT_SECRET`
- `APPLE_MUSIC_API_TOKEN`
- `DEEZER_ARL`
- `YANDEX_MUSIC_ACCESS_TOKEN`
- `YOUTUBE_REFRESH_TOKEN`

## YouTube Source

The config uses the external `dev.lavalink.youtube:youtube-plugin` source and disables Lavalink's built-in YouTube source:

```yaml
lavalink:
  server:
    sources:
      youtube: false
```

The YouTube plugin is configured under:

```yaml
plugins:
  youtube:
    enabled: true
    allowSearch: true
    allowDirectVideoIds: true
    allowDirectPlaylistIds: true
    oauth:
      enabled: true
      refreshToken: "${YOUTUBE_REFRESH_TOKEN:}"
```

OAuth helps with YouTube videos that require login, but it does not guarantee that every YouTube video will play from cloud-hosted IP addresses.

## YouTube OAuth Refresh Token

Do not commit a real YouTube OAuth refresh token.

For a deployed Lavalink instance, first start Lavalink without a `refreshToken`. The log will print a Google device login instruction similar to:

```text
Go to https://www.google.com/device and enter code XXXX-XXXX
```

Authorize with a burner Google account, then read the generated token from the Lavalink endpoint:

```sh
LAVALINK_PASSWORD_VALUE=$(grep '^LAVALINK_PASSWORD=' ../.env | cut -d= -f2-)
curl -s -H "Authorization: $LAVALINK_PASSWORD_VALUE" http://127.0.0.1:2333/youtube
```

On the deployment server only, add the token to the root `.env` file or deployment environment:

```env
YOUTUBE_REFRESH_TOKEN=your_refresh_token
```

Docker Compose passes this into Lavalink and `application.yaml` reads it as `${YOUTUBE_REFRESH_TOKEN:}`. Restart Lavalink after adding the token:

```sh
docker compose restart lavalink
```

If the token is valid, the log should include:

```text
YouTube access token refreshed successfully
```

## Search Prefixes

The bot resolver supports these prefixes:

- `youtube:` / `ytsearch:`
- `youtubemusic:` / `ytmsearch:`
- `soundcloud:` / `scsearch:`
- `spotify:` / `sptfy:`
- `applemusic:` / `amsearch:`
- `deezer:` / `dzsearch:`
- `yandexmusic:` / `ymsearch:`
- `bandcamp:` / `bcsearch:`

## Troubleshooting

Check Lavalink health:

```sh
curl -H "Authorization: $LAVALINK_PASSWORD" http://127.0.0.1:2333/version
```

Check YouTube search:

```sh
curl -sG \
  -H "Authorization: $LAVALINK_PASSWORD" \
  --data-urlencode "identifier=ytsearch:never gonna give you up" \
  http://127.0.0.1:2333/v4/loadtracks
```

Common YouTube failures:

- `This video requires login`
- `403` / `Forbidden`
- `All clients failed to load the item`
- `Video player configuration error`

These errors are source/playability problems, not bot queue or PostgreSQL errors. They can depend on the hosting provider IP address. Try a different result, SoundCloud/Spotify source, or run Lavalink from an IP range where the source is reachable.
