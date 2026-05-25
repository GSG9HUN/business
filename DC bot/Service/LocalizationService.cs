using System.Text.Json;
using DC_bot.Exceptions.Localization;
using DC_bot.Interface.Service.IO;
using DC_bot.Interface.Service.Localization;
using DC_bot.IO;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service;

public class LocalizationService : ILocalizationService
{
    private const string DefaultLanguage = "eng";

    private static readonly string LocalizationDirectory =
        Path.Combine(Directory.GetCurrentDirectory(), "guildFiles/localization");

    private static readonly string TranslationDirectory =
        Path.Combine(Directory.GetCurrentDirectory(), "localization");

    private readonly IFileSystem _fileSystem;
    private readonly Dictionary<ulong, Dictionary<string, string>> _guildTranslations = new();
    private readonly ILogger<LocalizationService> _logger;
    private readonly object _syncRoot = new();
    private Dictionary<string, string>? _defaultTranslations;

    public LocalizationService(ILogger<LocalizationService> logger, IFileSystem? fileSystem = null)
    {
        _fileSystem = fileSystem ?? new PhysicalFileSystem();
        if (!_fileSystem.DirectoryExists(LocalizationDirectory))
            _fileSystem.CreateDirectory(LocalizationDirectory);

        _logger = logger;
    }

    public string Get(string key, params object[] args)
    {
        return GetTranslation(GetDefaultTranslations(), key, args);
    }

    public string Get(ulong guildId, string key, params object[] args)
    {
        return GetTranslation(GetGuildTranslations(guildId), key, args);
    }

    public void LoadLanguage(ulong guildId)
    {
        var filePath = Path.Combine(LocalizationDirectory, $"{guildId}.json");
        var language = DefaultLanguage;

        if (_fileSystem.FileExists(filePath)) language = ReadJson<string>(filePath, "unknown") ?? DefaultLanguage;

        var translations = LoadTranslations(language);
        SetGuildTranslations(guildId, language, translations);
    }

    public void SaveLanguage(ulong guildId, string language)
    {
        var filePath = Path.Combine(LocalizationDirectory, $"{guildId}.json");
        var translations = LoadTranslations(language);

        WriteJson(filePath, language, language);

        SetGuildTranslations(guildId, language, translations);
    }

    private T? ReadJson<T>(string filePath, string languageCode)
    {
        try
        {
            var json = _fileSystem.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.LocalizationReadFailed(ex, filePath);
            throw new LocalizationException(languageCode, $"Failed to read JSON file: {filePath}", ex);
        }
    }

    private void WriteJson<T>(string filePath, T value, string languageCode)
    {
        try
        {
            _fileSystem.WriteAllText(filePath, JsonSerializer.Serialize(value));
        }
        catch (Exception ex)
        {
            _logger.LocalizationWriteFailed(ex, filePath);
            throw new LocalizationException(languageCode, $"Failed to write JSON file: {filePath}", ex);
        }
    }

    private Dictionary<string, string> LoadTranslations(string languageCode)
    {
        _logger.LocalizationLoading(languageCode);

        var filePath = Path.Combine(TranslationDirectory, $"{languageCode}.json");

        if (!_fileSystem.FileExists(filePath))
        {
            var exception = new FileNotFoundException($"Localization file not found: {filePath}");
            throw new LocalizationException(languageCode, $"Translation file not found: {filePath}", exception);
        }

        var translations = ReadJson<Dictionary<string, string>>(filePath, languageCode) ??
                           new Dictionary<string, string>();

        _logger.LocalizationLoaded();
        return translations;
    }

    private Dictionary<string, string> GetDefaultTranslations()
    {
        lock (_syncRoot)
        {
            if (_defaultTranslations is not null) return _defaultTranslations;
        }

        var translations = LoadTranslations(DefaultLanguage);

        lock (_syncRoot)
        {
            _defaultTranslations ??= translations;
            return _defaultTranslations;
        }
    }

    private Dictionary<string, string> GetGuildTranslations(ulong guildId)
    {
        lock (_syncRoot)
        {
            if (_guildTranslations.TryGetValue(guildId, out var translations)) return translations;
        }

        LoadLanguage(guildId);

        lock (_syncRoot)
        {
            return _guildTranslations[guildId];
        }
    }

    private void SetGuildTranslations(ulong guildId, string language, Dictionary<string, string> translations)
    {
        lock (_syncRoot)
        {
            _guildTranslations[guildId] = translations;
            if (language == DefaultLanguage) _defaultTranslations ??= translations;
        }
    }

    private static string GetTranslation(IReadOnlyDictionary<string, string> translations, string key, object[] args)
    {
        return translations.TryGetValue(key, out var value)
            ? string.Format(value, args)
            : key;
    }
}
