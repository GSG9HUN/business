

# Lavalink Server – Usage Guide

This folder contains everything you need to run a Lavalink server, including configuration, plugin setup, and a detailed explanation of what you need to provide for bot commands.

## 1. Required Files

- **Lavalink.jar**: Download the latest Lavalink (v4.x) JAR from https://github.com/lavalink-devs/Lavalink/releases
- **application.yaml**: This configuration file controls the server, sources, plugins, passwords, and API keys.

## 2. Starting the Server

1. Place `Lavalink.jar` in this folder.
2. Fill out `application.yaml` (see below).
3. Start the server:

```sh
java -jar Lavalink.jar
```

## 3. Main application.yaml Parameters

### Required:
- **server.port**: The server port (e.g., 2333)
- **lavalink.server.password**: This password must match the one in your bot!

### Plugins (recommended, already preconfigured):
- **lavasrc**: Multi-source search/playback (Spotify, Apple Music, Deezer, Yandex Music, etc.)
- **lavasearch**: Smart search, YouTube/ytmusic support
- **lavalyrics**: Lyrics search

### API Keys & Secrets:
- **Spotify**: `clientId`, `clientSecret` (from Spotify Developer Dashboard)
- **Apple Music**: `mediaAPIToken` (from Apple Developer account)
- **Deezer**: `masterDecryptionKey` or `arl` cookie
- **Yandex Music**: `accessToken` (optional but recommended)

### Source Enabling:
In `application.yaml`, under the `sources` section, you can enable which providers are available for bot commands (e.g., spotify, applemusic, deezer, yandexmusic, youtube, etc.).

### Audio Filters:
The following filters are supported if your bot can use them:
- volume, equalizer, karaoke, timescale, tremolo, vibrato, distortion, rotation, channelMix, lowPass

## 4. What to Provide in Bot Commands?

- **Password**: In your bot (e.g., `LavalinkSettings.cs`), use the same password as in application.yaml.
- **Port**: Set the server port in your bot (default: 2333).
- **Source Prefixes**: For search commands, you can use the following prefixes (if enabled):
  - `ytsearch:`, `ytmsearch:`, `scsearch:`, `spsearch:`, `amsearch:`, `dzsearch:`, `ymsearch:`, etc.
- **API Keys**: For Spotify, Apple Music, Deezer, Yandex Music features, you must provide your own keys/tokens in application.yaml.

## 5. Example application.yaml Snippet

```yaml
server:
  port: 2333
lavalink:
  plugins:
    - dependency: "com.github.topi314.lavasrc:lavasrc-plugin:4.4.0"
      repository: "https://maven.lavalink.dev/releases"
    - dependency: "com.github.topi314.lavasearch:lavasearch-plugin:1.5.1"
      repository: "https://maven.lavalink.dev/releases"
    - dependency: "com.github.topi314.lavalyrics:lavalyrics-plugin:1.0.1"
      repository: "https://maven.lavalink.dev/releases"
  server:
    password: "your_strong_password"
  sources:
    youtube: true
    spotify: true
    applemusic: true
    deezer: true
    yandexmusic: true
    # ...
  filters:
    volume: true
    equalizer: true
    # ...
  # API keys, see above
```

## 6. Troubleshooting & Tips

- If your bot cannot connect, check the password, port, and whether the Lavalink server is running.
- For plugin errors, check the server logs (`logs/` folder).
- Without API keys, related services will not work.

---

If you have questions about configuration or bot commands, check your bot's documentation or ask for help!
