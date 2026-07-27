using System.Text.RegularExpressions;

namespace DC_bot.Helper;

public static class DiscordTextSanitizer
{
    private static readonly Regex DangerousMentionPattern = new(
        @"@(everyone|here)\b|<@([!&]?\d+)>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static string EscapeMentions(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return DangerousMentionPattern.Replace(text, match =>
        {
            if (match.Value.StartsWith("<@", StringComparison.Ordinal))
            {
                return string.Concat("<@\u200B", match.Value.AsSpan(2));
            }

            return string.Concat("@\u200B", match.Value.AsSpan(1));
        });
    }
}
