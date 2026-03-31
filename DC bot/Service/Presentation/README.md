# Presentation Services

This folder contains services for building and sending Discord responses.

## Files

### ResponseBuilder.cs

**Purpose:** Build and send Discord messages consistently.

**Implements:** `IResponseBuilder`

**Methods:**

- `SendValidationErrorAsync()` - Send localized validation error
- `SendUsageAsync()` - Send command usage instructions
- `SendSuccessAsync()` - Send success message
- `SendCommandResponseAsync()` - Send command-specific response
- `SendCommandErrorResponse()` - Send command error message

**Internal Method:**

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
await responseBuilder.SendSuccessAsync(message, "Track added to queue");

// Command response
await responseBuilder.SendCommandResponseAsync(message, "play");
```

**Features:**

- Automatic localization lookup
- Error handling with try-catch
- Consistent formatting
- Wrapped `IDiscordMessage` usage

**Localization Integration:**

```csharp
// Retrieves from localization service
localization.Get(errorKey)
localization.Get($"{commandName}_command_response")
```

---

## Related Components

- **Interface/Service/Presentation/IResponseBuilder.cs** - Contract
- **Service/LocalizationService.cs** - Provides localized text
- **Commands/** - Use for sending responses
- **Wrapper/DiscordMessageWrapper.cs** - Message abstraction

