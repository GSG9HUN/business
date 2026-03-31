# Utility Commands

This folder contains general-purpose bot commands.

## Commands

### HelpCommand.cs

**Command:** `!help`

**Description:** List all available commands with descriptions.

**Behavior:**

1. Checks if user is a bot (returns early if true)
2. Retrieves all registered `ICommand` instances from DI
3. Builds response with command names and descriptions
4. Sends response to channel

**Implementation:**

```csharp
var commands = serviceProvider.GetServices<ICommand>();
var response = commands.Aggregate(string.Empty,
    (current, command) => current + $"{command.Name} : {command.Description}\n");
```

---

### PingCommand.cs

**Command:** `!ping`

**Description:** Check bot latency and response time.

**Behavior:**

1. Validates user is not a bot
2. Calculates bot's WebSocket ping
3. Sends response with latency information

---

### LanguageCommand.cs

**Command:** `!language <code>`

**Description:** Change the guild's language setting.

**Usage:**

```
!language en
!language hu
```

**Behavior:**

1. Validates user
2. Extracts language code from command arguments
3. Calls `ILocalizationService.SetLanguage(guildId, languageCode)`
4. Saves language preference to `guildFiles/localization/`

**Supported Languages:**

- `en` - English
- `hu` - Hungarian

---

### TagCommand.cs

**Command:** `!tag <action> [name] [content]`

**Description:** Manage custom text tags for the guild.

**Usage:**

```
!tag create welcome Welcome to our server!
!tag get welcome
!tag delete welcome
```

**Behavior:**

- Create/update tags for guild-specific content
- Retrieve stored tag content
- Delete existing tags

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
- `IServiceProvider` - Command discovery (HelpCommand)
- `ILogger<T>` - Logging

## Related Components

- `Service/LocalizationService.cs` - Language management
- `guildFiles/localization/` - Stored language preferences
- `Constants/LocalizationKeys.cs` - Localization key constants

