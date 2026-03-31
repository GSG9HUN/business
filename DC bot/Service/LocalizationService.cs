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
    internal static string LocalizationDirectory =
        Path.Combine(Directory.GetCurrentDirectory(), "guildFiles/localization");

    internal static string TranslationDirectory =
        Path.Combine(Directory.GetCurrentDirectory(), "localization");

    private readonly IFileSystem _fileSystem;

    private readonly ILogger<LocalizationService> _logger;
    private string? _lang;
    private Dictionary<string, string> _translations = new();

    public LocalizationService(ILogger<LocalizationService> logger, IFileSystem? fileSystem = null)
    {
        _fileSystem = fileSystem ?? new PhysicalFileSystem();
        if (!_fileSystem.DirectoryExists(LocalizationDirectory))
            _fileSystem.CreateDirectory(LocalizationDirectory);

        _logger = logger;
    }

    public string Get(string key, params object[] args)
    {
        return _translations.TryGetValue(key, out var value)
            ? string.Format(value, args)
            : key; // Ha nincs fordítás, akkor az eredeti kulcsot adja vissza
        // TODO: Ha egy kulcs hiányzik a fordítási fájlból, a Get() visszaadja magát a kulcsot (pl. "play_command_description").
        //       Ez nehézzé teszi a hiányzó fordítások észlelését éles környezetben. Javasolt legalább egy
        //       warning szintű log bejegyzés, ha egy kulcs nem található a szótárban.
    }

    public void LoadLanguage(ulong guildId)
    {
        var filePath = Path.Combine(LocalizationDirectory, $"{guildId}.json");

        if (!_fileSystem.FileExists(filePath))
        {
            LoadTranslations();
            return;
        }

        var lang = ReadJson<string>(filePath);
        _lang = lang ?? "eng";

        LoadTranslations(_lang);
    }

    public void SaveLanguage(ulong guildId, string language)
    {
        var filePath = Path.Combine(LocalizationDirectory, $"{guildId}.json");
        _lang = language;

        WriteJson(filePath, _lang);

        LoadTranslations(_lang);
    }

    private T? ReadJson<T>(string filePath)
    {
        try
        {
            var json = _fileSystem.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.LocalizationReadFailed(ex, filePath);
            throw new LocalizationException(_lang ?? "unknown", $"Failed to read JSON file: {filePath}", ex);
        }
    }

    private void WriteJson<T>(string filePath, T value)
    {
        try
        {
            _fileSystem.WriteAllText(filePath, JsonSerializer.Serialize(value));
        }
        catch (Exception ex)
        {
            _logger.LocalizationWriteFailed(ex, filePath);
            throw new LocalizationException(_lang ?? "unknown", $"Failed to write JSON file: {filePath}", ex);
        }
    }

    private void LoadTranslations(string languageCode = "eng")
    {
        _logger.LocalizationLoading(languageCode);

        var filePath = Path.Combine(TranslationDirectory, $"{languageCode}.json");

        if (!_fileSystem.FileExists(filePath))
        {
            var exception = new FileNotFoundException($"Localization file not found: {filePath}");
            throw new LocalizationException(languageCode, $"Translation file not found: {filePath}", exception);
        }

        _translations = ReadJson<Dictionary<string, string>>(filePath) ?? new Dictionary<string, string>();

        _logger.LocalizationLoaded();
    }
}