# Localization

This folder contains language translation files.

## Files

### eng.json

**Language:** English (default)

**Format:**
```json
{
  "clear_command_description": "Clear all queued tracks",
  "clear_command_response": "Queue cleared",
  "play_command_description": "Play music from URL or search query",
  "play_command_music_playing": "Now playing: {0}",
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
- user_not_in_voice_channel
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
4. Update localization service to load new language
5. Update configuration to support language selection

---

## Related Components

- **Constants/AppConstants.cs** - Key definitions
- **Service/LocalizationService.cs** - Key lookup
- **guildFiles/localization/** - Guild language preferences
- **Interface/Service/Localization/ILocalizationService.cs** - Localization contract

