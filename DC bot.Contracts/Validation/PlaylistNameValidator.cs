namespace DC_bot.Validation;

public static class PlaylistNameValidator
{
    const int MaxLength = 64;

    static bool IsValid(string playlistName)
    {
        return playlistName.Length is >= 1 and <= MaxLength
               && !playlistName.Contains('\n')
               && !playlistName.Contains('\r');
    }

    public static bool TryNormalize(string? playlistName, out string normalizedName)
    {
        normalizedName = playlistName?.Trim() ?? string.Empty;
        return IsValid(normalizedName);
    }
}
