# Validation Exceptions

This folder contains exceptions for validation failures.

## ValidationException

**Namespace:** `DC_bot.Exceptions.Validation`

**Properties:**
- `ValidationKey` (string) - Identifier for the validation rule that failed

**Definition:**
```csharp
public class ValidationException : BotException
{
    public string ValidationKey { get; }

    public ValidationException(string validationKey, string message) 
        : base(message)
    {
        ValidationKey = validationKey;
    }

    public ValidationException(string validationKey, string message, Exception innerException) 
        : base(message, innerException)
    {
        ValidationKey = validationKey;
    }
}
```

## Current Status

**Note:** This exception type is currently **not used** in the codebase. No services throw `ValidationException`.

The application uses **validation result objects** instead:
- `UserValidationResult` - For user validation
- `PlayerValidationResult` - For player state validation
- `ConnectionValidationResult` - For connection validation

These result objects are found in `Helper/Validation/` and returned by `ValidationService`.

## Related Files

- `Helper/Validation/UserValidationResult.cs`
- `Helper/Validation/PlayerValidationResult.cs`
- `Helper/Validation/ConnectionValidationResult.cs`
- `Service/Core/ValidationService.cs` - Uses result objects instead

