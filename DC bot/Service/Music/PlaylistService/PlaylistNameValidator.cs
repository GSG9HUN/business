namespace DC_bot.Service.Music.PlaylistService;

internal static class PlaylistNameValidator
{
    internal static bool IsValid(string playlistName)
    {
        return playlistName.Length is >= 1 and <= 64
               && !playlistName.Contains('\n')
               && !playlistName.Contains('\r');
    }
}
