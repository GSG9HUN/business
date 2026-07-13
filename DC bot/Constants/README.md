# Constants

This folder contains application-wide constants and localization keys.

## Files

### AppConstants.cs

**Purpose:** Central repository for localization key constants.

**Contents:**

Localization keys are grouped by feature or command. Constants must match keys in `localization/*.json`.

```csharp
public static class LocalizationKeys
{
    // Clear Command
    public const string ClearCommandDescription = "clear_command_description";
    public const string ClearCommandResponse = "clear_command_response";
    public const string ClearCommandConfirmationRequired = "clear_command_confirmation_required";

    // Help Command
    public const string HelpCommandDescription = "help_command_description";
    public const string HelpCommandResponse = "help_command_response";

    // Language Command
    public const string LanguageCommandInvalidLanguage = "language_command_invalid_language";

    // Play Command
    public const string PlayCommandDescription = "play_command_description";
    public const string PlayCommandMusicPlaying = "play_command_music_playing";
    public const string PlayCommandMusicAddedQueue = "play_command_music_added_queue";

    // Slash Commands
    public const string SlashCommandGuildOnly = "slash_command_guild_only";
    public const string SlashCommandDeferredAccepted = "slash_command_deferred_accepted";
    public const string SlashCommandNotRegistered = "slash_command_not_registered";
    public const string SlashCommandUnexpectedError = "slash_command_unexpected_error";
}

public static class ValidationErrorKeys
{
    public const string UserNotInVoiceChannel = "user_not_in_a_voice_channel";
    public const string BotIsNotConnectedError = "bot_is_not_connected_error";
    public const string LavalinkError = "lavalink_error";
}
```

**Usage:**

```csharp
// In commands
var description = localizationService.Get(LocalizationKeys.PlayCommandDescription);

// In validation
await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.UserNotInVoiceChannel);
```

**Benefits:**

- Type-safe key references
- Compile-time checking
- IDE auto-completion
- Easy refactoring

**Note:** Keys must match keys in `localization/*.json` files.

## Current Key Groups

- Clear, Help, Join, Language, Leave, Ping, Play, Pause, Resume, Skip, Shuffle, Repeat, RepeatList, Tag, ViewQueue, and Playlist command text.
- Playlist command text covers create, save, delete, add song, remove song, list, view, and rename keys.
- Reaction handler text for the music control message and repeat toggle responses.
- Unknown command response.
- Slash command fallback responses for guild-only usage, deferred accepted response, unregistered command, and unexpected errors.
- Validation error keys for user voice-channel state, Lavalink connectivity, and bot voice connection state.

---

## Localization Files

Keys defined here map to language files:

- `localization/eng.json` - English values
- `localization/hu.json` - Hungarian values

**Example JSON:**

```json
{
  "play_command_description": "Play music from URL or search query",
  "clear_command_response": "Playlist cleared.",
  "clear_command_confirmation_required": "Set confirm to true to clear the playlist.",
  "user_not_in_a_voice_channel": "You must be in a voice channel!",
  "slash_command_guild_only": "This command can only be used in a server."
}
```

## Related Components

- **localization/** - Language files
- **Service/LocalizationService.cs** - Key lookup
- **Commands/** - Use constants
- **Commands/TextCommands/Playlist/** - Playlist command localization users
- **Helper/Validation/** - Validation error keys
- **Service/SlashCommands/SlashCommandExecutor.cs** - Uses slash fallback keys

