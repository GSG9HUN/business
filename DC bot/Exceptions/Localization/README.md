# Localization Exceptions

This folder contains exceptions for localization system errors.

## LocalizationException

**Namespace:** `DC_bot.Exceptions.Localization`

**Properties:**
- `LanguageCode` (string) - The language code that caused the error

**When Thrown:**

### 1. File Read Failure
```csharp
// In LocalizationService.ReadJson<T>()
catch (Exception ex)
{
    throw new LocalizationException(_lang ?? "unknown", $"Failed to read JSON file: {filePath}", ex);
}
```

### 2. File Write Failure
```csharp
// In LocalizationService.WriteJson<T>()
catch (Exception ex)
{
    throw new LocalizationException(_lang ?? "unknown", $"Failed to write JSON file: {filePath}", ex);
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

Commands and services catch this exception to handle language loading failures:

```csharp
try
{
    await localizationService.LoadLanguageAsync(languageCode);
}
catch (LocalizationException ex)
{
    logger.LogWarning(ex, "Failed to load language: {LanguageCode}", ex.LanguageCode);
    // Fall back to default language
}
```

## Related Files

- `Service/LocalizationService.cs` - Throws this exception
- `localization/eng.json` - English translations
- `localization/hu.json` - Hungarian translations
- `guildFiles/localization/*.json` - Guild language preferences

