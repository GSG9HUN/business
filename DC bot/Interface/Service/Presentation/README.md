# Presentation Service Interfaces

This folder contains presentation-layer service interfaces.

## Files

### IResponseBuilder.cs

**Purpose:** Discord message response building.

```csharp
public interface IResponseBuilder
{
    Task SendValidationErrorAsync(IDiscordMessage message, string errorKey);
    Task SendUsageAsync(IDiscordMessage message, string commandName);
    Task SendSuccessAsync(IDiscordMessage message, string localizationKey, params object[] args);
    Task SendWarningAsync(IDiscordMessage message, string localizationKey, params object[] args);
    Task SendErrorAsync(IDiscordMessage message, string localizationKey, params object[] args);
}
```

**Methods:**

- `SendValidationErrorAsync()` - Send localized validation error
- `SendUsageAsync()` - Send command usage instructions
- `SendSuccessAsync()` - Send localized success message by key
- `SendWarningAsync()` - Send localized warning message by key with optional localized warning prefix
- `SendErrorAsync()` - Send localized error message by key with optional localized error prefix

**Implementation:** `Service/Presentation/ResponseBuilder.cs`

**Usage:**

```csharp
if (!result.IsValid)
{
    await responseBuilder.SendValidationErrorAsync(message, result.ErrorKey);
    return;
}

await responseBuilder.SendSuccessAsync(
    message,
    LocalizationKeys.CreatePlaylistCommandCreated,
    playlistName);
```

**Benefits:**

- Consistent message formatting
- Automatic localization
- Centralized error handling

---

## Related Components

- **Service/Presentation/ResponseBuilder.cs** - Implementation
- **Commands/** - Use for sending responses
- **Service/LocalizationService.cs** - Provides localized strings

