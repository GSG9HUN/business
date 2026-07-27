namespace DC_bot.Service.Music.PlaylistService;

internal static class PlaylistNameValidator
{
    internal const int MaxLength = 64;

    internal static bool IsValid(string playlistName)
    {
        return playlistName.Length is >= 1 and <= MaxLength
               && !playlistName.Contains('\n')
               && !playlistName.Contains('\r');
    }

    internal static bool TryNormalize(string? playlistName, out string normalizedName)
    {
        normalizedName = playlistName?.Trim() ?? string.Empty;
        return IsValid(normalizedName);
    }
}
