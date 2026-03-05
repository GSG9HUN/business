# Constants

This folder contains application-wide constants and localization keys.

## Files

### AppConstants.cs

**Purpose:** Central repository for localization key constants.

**Contents:**

Localization keys organized by command:

```csharp
public static class LocalizationKeys
{
    // Clear Command
    public const string ClearCommandDescription = "clear_command_description";
    public const string ClearCommandResponse = "clear_command_response";

    // Help Command
    public const string HelpCommandDescription = "help_command_description";
    public const string HelpCommandResponse = "help_command_response";

    // Play Command
    public const string PlayCommandDescription = "play_command_description";
    public const string PlayCommandMusicPlaying = "play_command_music_playing";
    public const string PlayCommandMusicAddedQueue = "play_command_music_added_queue";
    
    // ... more keys
}

public static class ValidationErrorKeys
{
    public const string UserNotInVoiceChannel = "user_not_in_voice_channel";
    public const string BotIsNotConnectedError = "bot_is_not_connected_error";
    public const string LavalinkError = "lavalink_error";
    // ... more keys
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

---

## Localization Files

Keys defined here map to language files:

- `localization/eng.json` - English values
- `localization/hu.json` - Hungarian values

**Example JSON:**
```json
{
  "play_command_description": "Play music from URL or search query",
  "clear_command_response": "Queue cleared",
  "user_not_in_voice_channel": "You must be in a voice channel"
}
```

## Related Components

- **localization/** - Language files
- **Service/LocalizationService.cs** - Key lookup
- **Commands/** - Use constants
- **Helper/Validation/** - Validation error keys

