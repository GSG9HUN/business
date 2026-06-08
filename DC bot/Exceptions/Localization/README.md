# Localization Exceptions

This folder contains exceptions for localization system errors.

## LocalizationException

**Namespace:** `DC_bot.Exceptions.Localization`

**Properties:**

- `LanguageCode` (string) - The language code that caused the error

**When Thrown:**

### 1. File Read Failure

```csharp
// In LocalizationService.ReadJson<T>(filePath, languageCode)
catch (Exception ex)
{
    throw new LocalizationException(languageCode, $"Failed to read JSON file: {filePath}", ex);
}
```

### 2. File Write Failure

```csharp
// In LocalizationService.WriteJson<T>(filePath, value, languageCode)
catch (Exception ex)
{
    throw new LocalizationException(languageCode, $"Failed to write JSON file: {filePath}", ex);
}
```

### 3. Translation File Not Found

```csharp
// In LocalizationService.LoadTranslations()
if (!_fileSystem.FileExists(filePath))
{
    throw new LocalizationException(languageCode, $"Translation file not found: {filePath}", exception);
}
```

## Usage in Code

### LocalizationService.cs

- Thrown when reading/writing JSON fails
- Thrown when language file doesn't exist
- Wraps underlying IOException or JsonException

## Handling

`LanguageCommand` catches this exception when saving a guild language fails. Other localization loading paths allow the
domain exception to bubble to the command handler or slash executor, where it is logged as a bot exception.

```csharp
try
{
    localizationService.SaveLanguage(message.Channel.Guild.Id, language);
}
catch (LocalizationException ex)
{
    logger.CommandExecutionFailed(ex, Name);
    await responseBuilder.SendCommandErrorResponse(message, Name);
}
```

## Related Files

- `Service/LocalizationService.cs` - Throws this exception
- `localization/eng.json` - English translations
- `localization/hu.json` - Hungarian translations
- `guildFiles/localization/*.json` - Guild language preferences

