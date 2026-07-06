# Utility Commands

This folder contains general-purpose bot commands.

## Commands

### HelpCommand.cs

**Command:** `!help`

**Description:** List all available commands with descriptions.

**Behavior:**

1. Checks if user is a bot (returns early if true)
2. Retrieves the stable registered command list from `ICommandRegistry`
3. Builds response with command names and descriptions
4. Sends response to channel

**Implementation:**

```csharp
var commands = commandRegistry.Commands;
var response = commands.Aggregate(string.Empty,
    (current, command) => current + $"{command.Name} : {command.Description}\n");
```

---

### PingCommand.cs

**Command:** `!ping`

**Description:** Reply with `Pong!`.

**Behavior:**

1. Validates user is not a bot
2. Sends the configured ping response

---

### LanguageCommand.cs

**Command:** `!language <code>`

**Description:** Change the guild's language setting.

**Usage:**

```
!language eng
!language hu
```

**Behavior:**

1. Validates user
2. Extracts language code from command arguments
3. Calls `ILocalizationService.SaveLanguage(guildId, languageCode)`
4. Saves language preference to `guildFiles/localization/`

**Supported Languages:**

- `eng` - English
- `hu` - Hungarian

**Supported languages:** `LanguageCommand` validates the requested code against its allowed language set before saving.
Unsupported codes return the localized invalid-language response.

---

### TagCommand.cs

**Command:** `!tag <username>`

**Description:** Mention a guild member by username.

**Usage:**

```
!tag Mario
```

**Behavior:**

- Looks up the provided username in the current guild
- Sends a localized mention response when the member exists
- Sends a localized error response when the member cannot be found

---

## Common Pattern

```csharp
public async Task ExecuteAsync(IDiscordMessage message)
{
    logger.CommandInvoked(Name);
    
    if (userValidation.IsBotUser(message))
    {
        return;
    }
    
    // Command logic
    
    logger.CommandExecuted(Name);
}
```

## Dependencies

- `IUserValidationService` - User validation
- `IResponseBuilder` - Message sending
- `ILocalizationService` - Language management
- `ICommandRegistry` - Command discovery (HelpCommand)
- `ILogger<T>` - Logging

## Related Components

- `Service/LocalizationService.cs` - Language management
- `guildFiles/localization/` - Stored language preferences
- `Constants/AppConstants.cs` - Localization key constants

