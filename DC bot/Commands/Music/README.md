# Music Commands

- `Helper/CommandHelper.cs` - Validation and argument extraction
- `Interface/ICommand.cs` - Command contract
- `Service/Music/MusicServices/` - Queue, repeat, track handling
- `Service/Music/LavaLinkService.cs` - Main playback orchestration

## Related Components

- `ILogger<T>` - Structured logging
- `ICommandHelper` - Command validation helpers
- `ILocalizationService` - Multi-language support
- `IResponseBuilder` - Discord message sending
- `IUserValidationService` - User/voice state validation
- `ILavaLinkService` - Music playback orchestration
All music commands inject:

## Dependencies

```
}
    logger.CommandExecuted(Name);

    await lavaLinkService.DoSomethingAsync();
    // Command-specific logic

    if (validationResult is null) return;
        userValidation, responseBuilder, message);
    var validationResult = await commandHelper.TryValidateUserAsync(
    
    logger.CommandInvoked(Name);
{
public async Task ExecuteAsync(IDiscordMessage message)
```csharp

All music commands follow this pattern:

## Common Pattern

---

5. Optionally saves queue state
4. Disconnects from voice channel
3. Stops playback
2. Calls `ILavaLinkService.LeaveVoiceChannel()`
1. Validates user
**Behavior:**

**Description:** Disconnect the bot from the voice channel.

**Command:** `!leave`
### LeaveCommand.cs

---

4. Starts playing queued tracks (if any)
3. Connects bot to user's voice channel
2. Calls `ILavaLinkService.StartPlayingQueue()`
1. Validates user is in a voice channel
**Behavior:**

**Description:** Make the bot join the user's voice channel.

**Command:** `!join`
### JoinCommand.cs

---

4. Plays next track in queue
3. Stops current track (triggers `TrackEnded` event)
2. Calls `ILavaLinkService.SkipAsync()`
1. Validates user
**Behavior:**

**Description:** Skip the current track and play the next in queue.

**Command:** `!skip`
### SkipCommand.cs

---

3. Resumes playback from paused position
2. Calls `ILavaLinkService.ResumeAsync()`
1. Validates user
**Behavior:**

**Description:** Resume a paused track.

**Command:** `!resume`
### ResumeCommand.cs

---

3. Pauses playback without clearing queue
2. Calls `ILavaLinkService.PauseAsync()`
1. Validates user
**Behavior:**

**Description:** Pause the currently playing track.

**Command:** `!pause`
### PauseCommand.cs

---

- Direct audio URLs
- SoundCloud URLs
- Spotify URLs
- YouTube URLs and searches
**Supported Sources:**

4. Adds track to queue or starts playback
3. Calls `ILavaLinkService.PlayAsyncUrl()` or `PlayAsyncQuery()`
2. Resolves query/URL via `ITrackSearchResolverService`
1. Validates user is in a voice channel
**Behavior:**

```
!play spotify:track:3cfOd4CMv2snFaKAnMdnvK
!play never gonna give you up
!play https://www.youtube.com/watch?v=dQw4w9WgXcQ
```
**Usage:**

**Description:** Play music from a URL or search query.

**Command:** `!play <query|url>`
### PlayCommand.cs

## Commands

This folder contains text commands for music playback control.

