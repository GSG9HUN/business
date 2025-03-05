using System.Text.Json;
using DC_bot.Interface;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service;

public class LocalizationService : ILocalizationService
{
    private Dictionary<string, string> _translations = new();

    private static readonly string LocalizationDirectory =
        Path.Combine(
            Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName ??
            throw new InvalidOperationException(), "guildFiles/localization");

    private static readonly string TranslationDirectory =
        Path.Combine(
            Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName ??
            throw new InvalidOperationException(), "localization");

    private ILogger<LocalizationService> _logger;
    private String? _lang;

    public LocalizationService(ILogger<LocalizationService> logger)
    {
        if (!Directory.Exists(LocalizationDirectory))
            Directory.CreateDirectory(LocalizationDirectory);

        _logger = logger;
    }

    private void LoadTranslations(string languageCode = "eng")
    {
        _logger.LogInformation($"Loading localization for {languageCode}");

        var filePath = Path.Combine(TranslationDirectory, $"{languageCode}.json");

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Localization file not found: {filePath}");

        var json = File.ReadAllText(filePath);

        _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ??
                        new Dictionary<string, string>();

        _logger.LogInformation("Localization loaded.");
    }

    public string Get(string key, params object[] args)
    {
        return _translations.TryGetValue(key, out var value)
            ? string.Format(value, args)
            : key; // Ha nincs fordítás, akkor az eredeti kulcsot adja vissza
    }

    public void LoadLanguage(ulong guildId)
    {
        var filePath = Path.Combine(LocalizationDirectory, $"{guildId}.json");

        if (!File.Exists(filePath))
        {
            LoadTranslations();
            return;
        }

        var lang = JsonSerializer.Deserialize<string>(File.ReadAllText(filePath));
        _lang = lang ?? "eng";

        LoadTranslations();
    }

    public void SaveLanguage(ulong guildId, string language)
    {
        var filePath = Path.Combine(LocalizationDirectory, $"{guildId}.json");
        _lang = language;
        
        File.WriteAllText(filePath, JsonSerializer.Serialize(_lang));
        
        LoadTranslations(_lang);
    }
}