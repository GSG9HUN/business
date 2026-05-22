namespace DC_bot.Interface.Service.Localization;

public interface ILocalizationService
{
    string Get(string key, params object[] args);

    string Get(ulong guildId, string key, params object[] args);

    void LoadLanguage(ulong guildId);

    void SaveLanguage(ulong guildId, string language);
}
