# Localization

This folder contains language translation files.

## Files

### eng.json

**Language:** English (default)

**Format:**

```json
{
  "clear_command_description": "Clear the playlist.",
  "clear_command_response": "Playlist cleared.",
  "clear_command_confirmation_required": "Set confirm to true to clear the playlist.",
  "play_command_description": "Start playing a music.",
  "play_command_music_playing": "Music is playing :",
  "slash_command_guild_only": "This command can only be used in a server.",
  ...
}
```

**Keys:** Match constants in `Constants/AppConstants.cs`

---

### hu.json

**Language:** Hungarian

**Format:** Same as `eng.json` but with Hungarian translations

---

## Key Naming Convention

```
{context}_{element}_{type}

- context: command name (play, pause, clear)
- element: what it describes (description, response, error)
- type: optional subtype (e.g., _error)

Examples:
- play_command_description
- pause_command_response
- skip_command_queue_is_empty
- user_not_in_a_voice_channel
- slash_command_unexpected_error
```

## Usage

```csharp
// Retrieve by key
var text = localizationService.Get(LocalizationKeys.PlayCommandDescription);

// With formatting
var message = localizationService.Get("play_command_music_playing", trackTitle);
```

## Adding New Languages

1. Create new JSON file (e.g., `es.json` for Spanish)
2. Copy all keys from `eng.json`
3. Translate values
4. Add the new code to `LanguageCommand.AllowedLanguageCodes`
5. Add a matching slash command choice if the language should be selectable through `/language`
6. Save the matching language code through `!language <code>` or `/language`

---

## Related Components

- **Constants/AppConstants.cs** - Key definitions
- **Service/LocalizationService.cs** - Key lookup
- **guildFiles/localization/** - Guild language preferences
- **Interface/Service/Localization/ILocalizationService.cs** - Localization contract

