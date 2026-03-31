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
    Task SendSuccessAsync(IDiscordMessage message, string text);
    Task SendCommandResponseAsync(IDiscordMessage message, string commandName);
    Task SendCommandErrorResponse(IDiscordMessage message, string commandName);
}
```

**Methods:**

- `SendValidationErrorAsync()` - Send localized validation error
- `SendUsageAsync()` - Send command usage instructions
- `SendSuccessAsync()` - Send success message
- `SendCommandResponseAsync()` - Send command-specific response
- `SendCommandErrorResponse()` - Send command error message

**Implementation:** `Service/Presentation/ResponseBuilder.cs`

**Usage:**

```csharp
// In commands
if (!result.IsValid)
{
    await responseBuilder.SendValidationErrorAsync(message, result.ErrorKey);
    return;
}

await responseBuilder.SendSuccessAsync(message, "Track added to queue");
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

