

# Lavalink Server – Usage Guide

This folder contains everything you need to run a Lavalink server, including configuration, plugin setup, and a detailed explanation of what you need to provide for bot commands.

## 1. Required Files

- **Lavalink.jar**: Required only for manual, non-Docker usage. Download a compatible Lavalink v4 JAR from https://github.com/lavalink-devs/Lavalink/releases
- **application.yaml**: This configuration file controls the server, sources, plugins, passwords, and API keys.

## 2. Starting the Server

1. Place `Lavalink.jar` in this folder.
2. Fill out `application.yaml` (see below).
3. Start the server:

```sh
java -jar Lavalink.jar
```

For the normal repository workflow, use Docker Compose from the repository root:

```sh
docker compose up --build
```

## 3. Main application.yaml Parameters

### Required:
- **server.port**: The server port (e.g., 2333)
- **lavalink.server.password**: This password must match the one in your bot!

### Plugins (recommended, already preconfigured):
- **lavasrc**: Multi-source search/playback (Spotify, Apple Music, Deezer, Yandex Music, etc.)
- **lavasearch**: Smart search, YouTube/ytmusic support
- **lavalyrics**: Lyrics search
- **youtube-plugin**: YouTube source support. The current config disables Lavalink's built-in YouTube source and uses the plugin path.

### API Keys & Secrets:
- **Spotify**: `clientId`, `clientSecret` (from Spotify Developer Dashboard)
- **Apple Music**: `mediaAPIToken` (from Apple Developer account)
- **Deezer**: `arl` cookie
- **Yandex Music**: `accessToken` (optional but recommended)

In this repository these are provided through environment variables:

- `SPOTIFY_CLIENT_ID`
- `SPOTIFY_CLIENT_SECRET`
- `APPLE_MUSIC_API_TOKEN`
- `DEEZER_ARL`
- `YANDEX_MUSIC_ACCESS_TOKEN`

### Source Enabling:
In `application.yaml`, provider availability is configured under `plugins.lavasrc.sources` for lavasrc providers and `lavalink.server.sources` for Lavalink's built-in sources.

### Audio Filters:
The following filters are supported if your bot can use them:
- volume, equalizer, karaoke, timescale, tremolo, vibrato, distortion, rotation, channelMix, lowPass

## 4. What to Provide in Bot Commands?

- **Password**: In your bot (e.g., `LavalinkSettings.cs`), use the same password as in application.yaml.
- **Port**: Set the server port in your bot (default: 2333).
- **Source Prefixes**: For search commands, the bot resolver supports:
  - `youtube:` / `ytsearch:`
  - `youtubemusic:` / `ytmsearch:`
  - `soundcloud:` / `scsearch:`
  - `spotify:` / `sptfy:`
  - `applemusic:` / `amsearch:`
  - `deezer:` / `dzsearch:`
  - `yandexmusic:` / `ymsearch:`
  - `bandcamp:` / `bcsearch:`
- **API Keys**: For Spotify, Apple Music, Deezer, and Yandex Music features, provide your own keys/tokens through the environment variables consumed by `application.yaml`.

## 5. Example application.yaml Snippet

```yaml
server:
  port: 2333
lavalink:
  plugins:
    - dependency: "dev.lavalink.youtube:youtube-plugin:1.18.0"
      repository: "https://maven.lavalink.dev/releases"
    - dependency: "com.github.topi314.lavasrc:lavasrc-plugin:4.8.1"
      repository: "https://maven.lavalink.dev/releases"
    - dependency: "com.github.topi314.lavasearch:lavasearch-plugin:1.0.0"
      repository: "https://maven.lavalink.dev/releases"
    - dependency: "com.github.topi314.lavalyrics:lavalyrics-plugin:1.1.0"
      repository: "https://maven.lavalink.dev/releases"
  server:
    password: "${LAVALINK_SERVER_PASSWORD:nagyon_eros_jelszo}"
  sources:
    youtube: false
    soundcloud: true
    bandcamp: true
    http: true
    spotify: true
    applemusic: false
    deezer: false
    yandexmusic: false
    # ...
  filters:
    volume: true
    equalizer: true
    # ...
  # API keys/environment variables, see above
```

## 6. Troubleshooting & Tips

- If your bot cannot connect, check the password, port, and whether the Lavalink server is running.
- For plugin errors, check the server logs (`logs/` folder).
- Without API keys, related services will not work.
- In Docker Compose, the bot reaches Lavalink at `lavalink:2333`.

---

If you have questions about configuration or bot commands, check your bot's documentation or ask for help!
