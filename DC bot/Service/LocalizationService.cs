using System.Text.Json;
using DC_bot.Exceptions.Localization;
using DC_bot.Interface.Service.IO;
using DC_bot.Interface.Service.Localization;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service;

/// <summary>
/// Loads language dictionaries and persists guild language preferences through the file-system boundary.
/// </summary>
public class LocalizationService : ILocalizationService
{
    private const string DefaultLanguage = "en";
    private static readonly string LocalizationDirectory = Path.Combine(Directory.GetCurrentDirectory(), "guildFiles/localization");
    private static readonly string TranslationDirectory = Path.Combine(Directory.GetCurrentDirectory(), "localization");
    private readonly IFileSystem _fileSystem;
    private readonly Dictionary<ulong, Dictionary<string, string>> _guildTranslations = new();
    private readonly ILogger<LocalizationService> _logger;
    private readonly object _syncRoot = new();
    private Dictionary<string, string>? _defaultTranslations;

    /// <summary>
    /// Initializes LocalizationService with the supplied dependencies and settings.
    /// </summary>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    /// <param name="fileSystem">The file system dependency used by this boundary.</param>
    public LocalizationService(ILogger<LocalizationService> logger, IFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

        if (!_fileSystem.DirectoryExists(LocalizationDirectory))
        {
            _fileSystem.CreateDirectory(LocalizationDirectory);
        }

        _logger = logger;
    }

    /// <summary>
    /// Resolves a localized value for the supplied key and formatting arguments.
    /// </summary>
    /// <param name="key">The key supplied for this operation.</param>
    /// <param name="args">The args supplied for this operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    public string Get(string key, params object[] args)
    {
        return GetTranslation(GetDefaultTranslations(), key, args);
    }

    /// <summary>
    /// Resolves a localized value for the supplied key and formatting arguments.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="key">The key supplied for this operation.</param>
    /// <param name="args">The args supplied for this operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    public string Get(ulong guildId, string key, params object[] args)
    {
        return GetTranslation(GetGuildTranslations(guildId), key, args);
    }

    /// <summary>
    /// Loads the guild&apos;s saved language preference and dictionary.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    public void LoadLanguage(ulong guildId)
    {
        var filePath = Path.Combine(LocalizationDirectory, $"{guildId}.json");
        var language = DefaultLanguage;

        if (_fileSystem.FileExists(filePath))
        {
            language = ReadJson<string>(filePath, "unknown") ?? DefaultLanguage;
        }

        var translations = LoadTranslations(language);
        SetGuildTranslations(guildId, language, translations);
    }

    /// <summary>
    /// Persists the guild&apos;s language preference and updates cached translations.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="language">The language supplied for this operation.</param>
    public void SaveLanguage(ulong guildId, string language)
    {
        var filePath = Path.Combine(LocalizationDirectory, $"{guildId}.json");
        var translations = LoadTranslations(language);
        WriteJson(filePath, language, language);
        SetGuildTranslations(guildId, language, translations);
    }

    /// <summary>
    /// Reads and deserializes localization data while reporting file and parse failures.
    /// </summary>
    /// <param name="filePath">The file path supplied for this operation.</param>
    /// <param name="languageCode">The language code supplied for this operation.</param>
    /// <typeparam name = "T">The type of value consumed or produced by the operation.</typeparam>
    /// <returns>The value produced for the supplied operation context.</returns>
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

    /// <summary>
    /// Serializes and writes localization data while reporting write failures.
    /// </summary>
    /// <param name="filePath">The file path supplied for this operation.</param>
    /// <param name="value">The value supplied for this operation.</param>
    /// <param name="languageCode">The language code supplied for this operation.</param>
    /// <typeparam name = "T">The type of value consumed or produced by the operation.</typeparam>
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

    /// <summary>
    /// Reads a language dictionary and applies the fallback policy.
    /// </summary>
    /// <param name="languageCode">The language code supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    private Dictionary<string, string> LoadTranslations(string languageCode)
    {
        _logger.LocalizationLoading(languageCode);
        var filePath = Path.Combine(TranslationDirectory, $"{languageCode}.json");

        if (!_fileSystem.FileExists(filePath))
        {
            var exception = new FileNotFoundException($"Localization file not found: {filePath}");
            throw new LocalizationException(languageCode, $"Translation file not found: {filePath}", exception);
        }

        var translations = ReadJson<Dictionary<string, string>>(filePath, languageCode) ?? new Dictionary<string, string>();
        _logger.LocalizationLoaded();
        return translations;
    }

    /// <summary>
    /// Returns the cached default dictionary, loading it when first needed.
    /// </summary>
    /// <returns>The value produced for the supplied operation context.</returns>
    private Dictionary<string, string> GetDefaultTranslations()
    {
        lock (_syncRoot)
        {
            if (_defaultTranslations is not null)
            {
                return _defaultTranslations;
            }
        }

        var translations = LoadTranslations(DefaultLanguage);
        lock (_syncRoot)
        {
            _defaultTranslations ??= translations;
            return _defaultTranslations;
        }
    }

    /// <summary>
    /// Returns cached guild translations, loading the saved language when needed.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    private Dictionary<string, string> GetGuildTranslations(ulong guildId)
    {
        lock (_syncRoot)
        {
            if (_guildTranslations.TryGetValue(guildId, out var translations))
            {
                return translations;
            }
        }

        LoadLanguage(guildId);
        lock (_syncRoot)
        {
            return _guildTranslations[guildId];
        }
    }

    /// <summary>
    /// Updates the guild&apos;s language and translation cache under synchronization.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="language">The language supplied for this operation.</param>
    /// <param name="translations">The translations supplied for this operation.</param>
    private void SetGuildTranslations(ulong guildId, string language, Dictionary<string, string> translations)
    {
        lock (_syncRoot)
        {
            _guildTranslations[guildId] = translations;

            if (language == DefaultLanguage)
            {
                _defaultTranslations ??= translations;
            }
        }
    }

    /// <summary>
    /// Resolves a translation with fallback handling and formatting arguments.
    /// </summary>
    /// <param name="translations">The translations supplied for this operation.</param>
    /// <param name="key">The key supplied for this operation.</param>
    /// <param name="args">The args supplied for this operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    private static string GetTranslation(IReadOnlyDictionary<string, string> translations, string key, object[] args)
    {
        return translations.TryGetValue(key, out var value) ? string.Format(value, args) : key;
    }
}
