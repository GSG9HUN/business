namespace DC_bot.Interface.Service.Localization;

/// <summary>
/// Defines the localization service boundary so callers are independent of its concrete implementation.
/// </summary>
public interface ILocalizationService
{

    /// <summary>
    /// Resolves a localized value for the supplied key and formatting arguments.
    /// </summary>
    /// <param name="key">The key supplied for this operation.</param>
    /// <param name="args">The args supplied for this operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    string Get(string key, params object[] args);

    /// <summary>
    /// Resolves a localized value for the supplied key and formatting arguments.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="key">The key supplied for this operation.</param>
    /// <param name="args">The args supplied for this operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    string Get(ulong guildId, string key, params object[] args);

    /// <summary>
    /// Loads the guild&apos;s saved language preference and dictionary.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    void LoadLanguage(ulong guildId);

    /// <summary>
    /// Persists the guild&apos;s language preference and updates cached translations.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="language">The language supplied for this operation.</param>
    void SaveLanguage(ulong guildId, string language);
}
