# Constants

This folder contains application-wide constant values and localization keys.

## What's here?

Centralized constant definitions that are used throughout the application to:
- Avoid magic strings
- Provide compile-time checking
- Enable easy refactoring
- Support localization

## Contents

### AppConstants.cs

Contains nested static classes for different constant categories:

#### LocalizationKeys
String keys used to retrieve localized text from language files (`localization/eng.json`, `localization/hu.json`).

**Example:**
```csharp
public static class LocalizationKeys
{
    // Command descriptions
    public const string PlayCommandDescription = "play_command_description";
    public const string PauseCommandDescription = "pause_command_description";
    
    // Command responses
    public const string PingCommandResponse = "Pong!";
    public const string PauseCommandResponse = "pause_command_response";
    
    // Error messages
    public const string PlayCommandFailedToFindMusicUrlError = "play_command_failed_to_find_music_url_error";
    public const string UserNotInVoiceChannel = "user_not_in_a_voice_channel";
}
```

**Usage:**
```csharp
// In a command
var description = localizationService.Get(LocalizationKeys.PlayCommandDescription);
await channel.SendMessageAsync(description);

// In a service
logger.LogInformation(LocalizationKeys.PauseCommandResponse);
```

#### ValidationErrorKeys
Keys for validation error messages:

```csharp
public static class ValidationErrorKeys
{
    public const string LavalinkError = "lavalink_error";
    public const string BotNotConnectedError = "bot_is_not_connected_error";
    public const string UserNotInVoiceChannel = "user_not_in_a_voice_channel";
    public const string UserIsBot = "user_is_bot";
}
```

**Usage:**
```csharp
if (!validationResult.IsValid)
{
    await responseBuilder.SendValidationErrorAsync(
        message, 
        ValidationErrorKeys.UserNotInVoiceChannel);
}
```

#### EventIds (if present)
Structured logging event IDs for categorizing logs:

```csharp
public static class EventIds
{
    public const int CommandInvoked = 1000;
    public const int CommandExecuted = 1001;
    public const int CommandFailed = 1002;
    
    public const int LavalinkConnected = 2000;
    public const int LavalinkDisconnected = 2001;
}
```

See also: `Logging/EventIdTable.md`

## Why Constants?

### ❌ Without Constants (Bad)
```csharp
// Typos at runtime
await channel.SendMessageAsync("paly_command_description"); // TYPO!

// Inconsistent strings
await channel.SendMessageAsync("play_command_desc");
await otherChannel.SendMessageAsync("play_description"); // INCONSISTENT!

// Hard to refactor
// Find all usages of "play_command_description" across 50 files? Good luck!
```

### ✅ With Constants (Good)
```csharp
// Compile-time checking
await channel.SendMessageAsync(LocalizationKeys.PlayCommandDescription); // ✅

// Consistent usage
await channel.SendMessageAsync(LocalizationKeys.PlayCommandDescription);
await otherChannel.SendMessageAsync(LocalizationKeys.PlayCommandDescription); // ✅

// Easy refactoring
// Rename LocalizationKeys.PlayCommandDescription → IDE updates all usages!
```

## Organization

Constants are organized by domain:

```
AppConstants.cs
├─ LocalizationKeys
│  ├─ Command descriptions
│  ├─ Command responses
│  ├─ Error messages
│  └─ UI text
├─ ValidationErrorKeys
│  ├─ User validation errors
│  ├─ Player validation errors
│  └─ Connection errors
└─ EventIds (optional)
   ├─ Command events
   ├─ Service events
   └─ System events
```

## Localization Integration

Constants work with the localization system:

**1. Define constant:**
```csharp
public const string PlayCommandDescription = "play_command_description";
```

**2. Add to language files:**

`localization/eng.json`:
```json
{
  "play_command_description": "Play music from URL or search query"
}
```

`localization/hu.json`:
```json
{
  "play_command_description": "Zene lejátszása URL-ből vagy keresési lekérdezésből"
}
```

**3. Use in code:**
```csharp
var description = localizationService.Get(LocalizationKeys.PlayCommandDescription);
// Returns: "Play music from URL or search query" (English)
// or: "Zene lejátszása URL-ből vagy keresési lekérdezésből" (Hungarian)
```

## Best Practices

- ✅ Use constants for all user-facing text
- ✅ Use constants for validation error keys
- ✅ Use constants for event IDs (logging)
- ✅ Group related constants together
- ✅ Use descriptive names (not abbreviations)
- ✅ Use PascalCase for constant names
- ✅ Use UPPER_SNAKE_CASE for event IDs (convention)
- ❌ Don't use constants for business logic values (use configuration)
- ❌ Don't use constants for computed values
- ❌ Don't create constants for single-use strings

## Adding New Constants

**1. Identify the constant type:**
- User-facing text? → `LocalizationKeys`
- Validation error? → `ValidationErrorKeys`
- Log event? → `EventIds`

**2. Add to appropriate section:**
```csharp
public static class LocalizationKeys
{
    // ...existing constants...
    
    // New command
    public const string MyNewCommandDescription = "my_new_command_description";
    public const string MyNewCommandResponse = "my_new_command_response";
}
```

**3. Add to localization files:**
```json
{
  "my_new_command_description": "English description",
  "my_new_command_response": "English response"
}
```

**4. Use in code:**
```csharp
var desc = localizationService.Get(LocalizationKeys.MyNewCommandDescription);
```

## Testing

Constants make testing easier:

```csharp
[Fact]
public async Task Command_Should_Send_Correct_Message()
{
    // Arrange
    var expectedKey = LocalizationKeys.PlayCommandDescription;
    
    // Act
    await command.ExecuteAsync(message);
    
    // Assert
    mockLocalization.Verify(x => x.Get(expectedKey), Times.Once);
}
```

## Related

- **localization/** - Language files (eng.json, hu.json)
- **Service/LocalizationService.cs** - Localization implementation
- **Interface/ILocalizationService.cs** - Localization contract
- **Logging/EventIdTable.md** - Event ID reference table

