using System.Text.RegularExpressions;

namespace DC_bot.Helper;

/// <summary>
/// Escapes user-controlled text that could trigger unintended Discord mentions.
/// </summary>
public static class DiscordTextSanitizer
{
    private static readonly Regex DangerousMentionPattern = new(@"@(everyone|here)\b|<@([!&]?\d+)>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// Escapes mention syntax in user-controlled text before Discord presentation.
    /// </summary>
    /// <param name="text">The text supplied for this operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
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

    /// <summary>Limits Discord text without cutting a UTF-16 surrogate pair, adding an ellipsis when shortened.</summary>
    /// <param name="text">The text to bound.</param>
    /// <param name="maxLength">The maximum UTF-16 length, including the ellipsis.</param>
    /// <returns>The original or bounded text.</returns>
    public static string Truncate(string? text, int maxLength)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxLength, 3);
        text ??= string.Empty;

        if (text.Length <= maxLength)
        {
            return text;
        }

        var length = maxLength - 3;

        if (length > 0
            && char.IsHighSurrogate(text[length - 1])
            && char.IsLowSurrogate(text[length]))
        {
            length--;
        }

        return string.Concat(text.AsSpan(0, length), "...");
    }

}
