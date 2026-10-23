namespace DC_bot.Validation;

/// <summary>
/// Normalizes playlist names to 1–64 trimmed characters while rejecting carriage returns and line feeds.
/// </summary>
public static class PlaylistNameValidator
{
    const int MaxLength = 64;

    /// <summary>
    /// Checks whether the supplied name or theme satisfies its documented contract constraints.
    /// </summary>
    /// <param name = "playlistName">The stored guild-owned playlist name.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    static bool IsValid(string playlistName)
    {
        return playlistName.Length is >= 1 and <= MaxLength && !playlistName.Contains('\n') && !playlistName.Contains('\r');
    }

    /// <summary>
    /// Trims and validates playlist names or normalizes supported mobile language codes, returning false for unsupported input.
    /// </summary>
    /// <param name = "playlistName">The stored guild-owned playlist name.</param>
    /// <param name = "normalizedName">The normalized output; empty when validation fails.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    public static bool TryNormalize(string? playlistName, out string normalizedName)
    {
        normalizedName = playlistName?.Trim() ?? string.Empty;
        return IsValid(normalizedName);
    }
}
