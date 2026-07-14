# Presentation Services

This folder contains services for building and sending Discord responses.

## Files

### ResponseBuilder.cs

**Purpose:** Build and send Discord messages consistently.

**Implements:** `IResponseBuilder`

**Methods:**

- `SendValidationErrorAsync()` - Send localized validation error
- `SendUsageAsync()` - Send command usage instructions
- `SendSuccessAsync()` - Send localized success message by key
- `SendWarningAsync()` - Send localized warning message by key with optional localized warning prefix
- `SendErrorAsync()` - Send localized error message by key with optional localized error prefix

**Internal Method:**

- `GetForMessage()` - Resolve a localization key using the guild from the wrapped message
- `FormatWithPrefix()` - Add a localized warning or error prefix when configured
- `SafeRespondAsync()` - Send message with error handling

**Usage:**

```csharp
// Validation error
if (!result.IsValid)
{
    await responseBuilder.SendValidationErrorAsync(message, result.ErrorKey);
    return;
}

// Success
await responseBuilder.SendSuccessAsync(
    message,
    LocalizationKeys.AddSongToPlaylistCommandAdded,
    playlistName);

// Warning
await responseBuilder.SendWarningAsync(
    message,
    LocalizationKeys.CreatePlaylistCommandAlreadyExists,
    playlistName);

// Error
await responseBuilder.SendErrorAsync(
    message,
    LocalizationKeys.CreatePlaylistCommandUnknownError,
    playlistName);
```

**Features:**

- Guild-aware localization lookup through the wrapped message channel
- Optional `response_warning_prefix` and `response_error_prefix` localization prefixes
- Error handling with try-catch
- Consistent formatting
- Wrapped `IDiscordMessage` usage
- Empty validation error keys are ignored

**Localization Integration:**

```csharp
var guildId = message.Channel.Guild.Id;
var text = localization.Get(guildId, key, args);
```

---

## Related Components

- **Interface/Service/Presentation/IResponseBuilder.cs** - Contract
- **Service/LocalizationService.cs** - Provides localized text
- **Commands/** - Use for sending responses
- **Wrapper/DiscordMessageWrapper.cs** - Message abstraction

