# Reaction Handler Services

This folder contains Discord reaction-control handling for now-playing messages.

## Purpose

Reaction controls let users drive playback from Discord message reactions. The handler layer translates reaction events into playback actions while keeping Discord event payloads away from music services.

## Files

### ReactionHandlerService.cs

Registers Discord reaction events and delegates valid control reactions.

### ReactionContextFactory.cs

Converts DSharpPlus reaction event payloads into a `ReactionContext` with wrapped guild/channel/member/message data.

### ReactionContext.cs

Carries the resolved context required by reaction actions.

### ReactionActionDispatcher.cs

Maps normalized control emojis to music actions such as pause/resume, skip, repeat, and queue navigation.

### ReactionControlMessageService.cs

Builds and publishes the now-playing control message and attaches playback control emojis.

### ReactionControlEmojis.cs

Central list of supported playback reaction emojis and normalization helpers.

## Flow

```text
TrackNotificationService.TrackStarted
  -> ReactionControlMessageService publishes controls
Discord reaction event
  -> ReactionHandlerService
  -> ReactionContextFactory
  -> ReactionActionDispatcher
  -> music service action
```

## Notes

- Keep Lavalink/player validation in music services.
- Reaction handlers should focus on Discord event routing and context creation.
