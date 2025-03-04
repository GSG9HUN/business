namespace DC_bot.Interface;

public interface ILocalizationService
{
    private static readonly string LocalizationDirectory =
        Path.Combine(
            Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName ??
            throw new InvalidOperationException(), "guildFiles/localization");

    public string Get(string key, params object[] args);

    public void LoadLanguage(ulong guildId);

    public void SaveLanguage(ulong guildId, string language);
}