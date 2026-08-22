namespace DC_bot.Interface.Service.Persistence.Models.MobileAppUserSettings;

public static class MobileAppLanguageCode
{
    public const int MaxLength = 10;
    public const string English = "en";
    public const string Hungarian = "hu";

    private static readonly string[] SupportedCodes = [English, Hungarian];
    private static readonly HashSet<string> AllowedCodes = new(SupportedCodes, StringComparer.Ordinal);

    public static string SupportedValues { get; } = string.Join(", ", SupportedCodes);

    public static bool TryNormalize(string? languageCode, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return false;
        }

        var candidate = languageCode.Trim().ToLowerInvariant();
        if (candidate.Length > MaxLength || !AllowedCodes.Contains(candidate))
        {
            return false;
        }

        normalized = candidate;
        return true;
    }
}
