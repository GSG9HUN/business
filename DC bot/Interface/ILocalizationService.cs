namespace DC_bot.Interface;

public interface ILocalizationService
{
    string Get(string key, params object[] args);

    void LoadLanguage(ulong guildId);

    void SaveLanguage(ulong guildId, string language);
}