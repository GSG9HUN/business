namespace DC_bot.Exceptions.Localization;

/// <summary>
/// Exception thrown when localization operations fail.
/// </summary>
public class LocalizationException : BotException
{
    public string LanguageCode { get; }

    public LocalizationException(string languageCode, string message) 
        : base($"Localization error for language '{languageCode}': {message}")
    {
        LanguageCode = languageCode;
    }

    public LocalizationException(string languageCode, string message, Exception innerException) 
        : base($"Localization error for language '{languageCode}': {message}", innerException)
    {
        LanguageCode = languageCode;
    }
}

