namespace DC_bot.Exceptions.Localization;

/// <summary>
///     Exception thrown when localization operations fail.
/// </summary>
public class LocalizationException : BotException
{

    /// <summary>
    /// Initializes LocalizationException with the supplied dependencies and settings.
    /// </summary>
    /// <param name="languageCode">The language code supplied for this operation.</param>
    /// <param name="message">The message supplied for this operation.</param>
    public LocalizationException(string languageCode, string message) : base($"Localization error for language '{languageCode}': {message}")
    {
        LanguageCode = languageCode;
    }

    /// <summary>
    /// Initializes LocalizationException with the supplied dependencies and settings.
    /// </summary>
    /// <param name="languageCode">The language code supplied for this operation.</param>
    /// <param name="message">The message supplied for this operation.</param>
    /// <param name="innerException">The inner exception supplied for this operation.</param>
    public LocalizationException(string languageCode, string message, Exception innerException) : base($"Localization error for language '{languageCode}': {message}", innerException)
    {
        LanguageCode = languageCode;
    }

    /// <summary>
    /// The language code involved in the failed localization operation.
    /// </summary>
    public string LanguageCode { get; }
}
