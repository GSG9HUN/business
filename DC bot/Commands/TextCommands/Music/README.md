# Music Commands

This folder contains text commands for music playback control.

## Commands

### PlayCommand.cs

**Command:** `!play <query|url>`

**Description:** Play music from a URL or search query.

**Usage:**

```
!play https://www.youtube.com/watch?v=dQw4w9WgXcQ
!play never gonna give you up
!play spotify:track:3cfOd4CMv2snFaKAnMdnvK
```

**Behavior:**

1. Validates user is in a voice channel
2. Extracts the query/URL argument through `ICommandHelper`
3. Resolves query/URL via `ITrackSearchResolverService`
4. Calls `ILavaLinkService.PlayAsyncUrl()` or `PlayAsyncQuery()`
5. Starts playback or queues the loaded track through the music services

**Supported Sources:** YouTube, YouTube Music, Spotify, SoundCloud, Apple Music, Deezer, Yandex Music, Bandcamp, and direct URLs supported by Lavalink.

---

### PauseCommand.cs

**Command:** `!pause`

**Description:** Pause the currently playing track.

**Behavior:**

1. Validates user
2. Calls `ILavaLinkService.PauseAsync()`
3. Pauses playback without clearing queue state

---

### ResumeCommand.cs

**Command:** `!resume`

**Description:** Resume a paused track.

**Behavior:**

1. Validates user
2. Calls `ILavaLinkService.ResumeAsync()`
3. Resumes playback from paused position

---

### SkipCommand.cs

**Command:** `!skip`

**Description:** Skip the current track and continue with the next queued item when one exists.

**Behavior:**

1. Validates user
2. Calls `ILavaLinkService.SkipAsync()`
3. Stops the current track and lets the playback event flow advance the queue

---

### JoinCommand.cs

**Command:** `!join`

**Description:** Make the bot join the user's voice channel and start queued playback.

**Behavior:**

1. Validates user is in a voice channel
2. Calls `ILavaLinkService.StartPlayingQueue()`
3. Joins the user's voice channel
4. Starts playing queued tracks if any are available

---

### LeaveCommand.cs

**Command:** `!leave`

**Description:** Disconnect the bot from the voice channel.

**Behavior:**

1. Validates user
2. Calls `ILavaLinkService.LeaveVoiceChannel()`
3. Stops playback if needed
4. Cleans up playback handlers/timer state and disconnects

---

## Common Pattern

All music commands follow this pattern:

```csharp
public async Task ExecuteAsync(IDiscordMessage message)
{
    logger.CommandInvoked(Name);

    var validationResult = await commandHelper.TryValidateUserAsync(
        userValidation, responseBuilder, message);
    if (validationResult is null) return;

    // Command-specific logic
    await lavaLinkService.DoSomethingAsync();

    logger.CommandExecuted(Name);
}
```

## Dependencies

All music commands inject a subset of:

- `ILogger<T>` - Structured logging
- `ICommandHelper` - Command validation and argument helpers
- `ILocalizationService` - Multi-language support
- `IResponseBuilder` - Discord response sending
- `IUserValidationService` - User/voice state validation
- `ILavaLinkService` - Music playback orchestration
- `ITrackSearchResolverService` - Query/URL search mode resolution

## Related Components

- `Interface/ICommand.cs` - Command contract
- `Interface/Core/ICommandHelper.cs` - Command helper contract
- `Service/Core/CommandValidationService.cs` - Command helper implementation
- `Service/Music/MusicServices/` - Queue, repeat, track handling
- `Service/Music/LavaLinkService.cs` - Main playback orchestration
