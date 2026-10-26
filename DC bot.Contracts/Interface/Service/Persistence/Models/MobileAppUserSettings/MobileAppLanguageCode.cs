namespace DC_bot.Interface.Service.Persistence.Models.MobileAppUserSettings;

/// <summary>
/// Defines supported mobile language codes and their case-insensitive input normalization.
/// </summary>
public static class MobileAppLanguageCode
{

    /// <summary>
    /// The maximum length of a supported language code.
    /// </summary>
    public const int MaxLength = 10;

    /// <summary>
    /// The supported English language code.
    /// </summary>
    public const string English = "en";

    /// <summary>
    /// The supported Hungarian language code.
    /// </summary>
    public const string Hungarian = "hu";
    private static readonly string[] SupportedCodes = [English, Hungarian];
    private static readonly HashSet<string> AllowedCodes = new(SupportedCodes, StringComparer.Ordinal);

    /// <summary>
    /// The language codes accepted by application settings validation.
    /// </summary>
    public static string SupportedValues { get; } = string.Join(", ", SupportedCodes);

    /// <summary>
    /// Trims and validates playlist names or normalizes supported mobile language codes, returning false for unsupported input.
    /// </summary>
    /// <param name = "languageCode">The language code value used by this storage contract.</param>
    /// <param name = "normalized">The normalized output; empty when validation fails.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    public static bool TryNormalize(string? languageCode, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return false;
        }

        var candidate = languageCode.Trim()
            .ToLowerInvariant();

        if (candidate.Length > MaxLength || !AllowedCodes.Contains(candidate))
        {
            return false;
        }

        normalized = candidate;
        return true;
    }
}
