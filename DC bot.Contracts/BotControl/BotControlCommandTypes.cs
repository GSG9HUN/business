namespace DC_bot.BotControl;

/// <summary>
/// Defines stable command type names for the durable API-to-bot command bridge.
/// </summary>
public static class BotControlCommandTypes
{

    /// <summary>
    /// The bot control command type for mentioning a user.
    /// </summary>
    public const string Tag = "tag";

    /// <summary>
    /// The bot control command type for listing available commands.
    /// </summary>
    public const string Help = "help";

    /// <summary>
    /// The bot control command type for checking bot responsiveness.
    /// </summary>
    public const string Ping = "ping";

    /// <summary>
    /// The bot control command type for changing the guild language.
    /// </summary>
    public const string Language = "language";

    /// <summary>
    /// The bot control command type for joining a voice channel.
    /// </summary>
    public const string Join = "join";

    /// <summary>
    /// The bot control command type for resolving and playing music.
    /// </summary>
    public const string Play = "play";

    /// <summary>
    /// The bot control command type for pausing playback.
    /// </summary>
    public const string Pause = "pause";

    /// <summary>
    /// The bot control command type for resuming playback.
    /// </summary>
    public const string Resume = "resume";

    /// <summary>
    /// The bot control command type for skipping the current track.
    /// </summary>
    public const string Skip = "skip";

    /// <summary>
    /// The bot control command type for playing the previous track.
    /// </summary>
    public const string Previous = "previous";

    /// <summary>
    /// The bot control command type for leaving the voice channel.
    /// </summary>
    public const string Leave = "leave";

    /// <summary>
    /// The bot control command type for stopping playback through the leave command alias.
    /// </summary>
    public const string Stop = "stop";

    /// <summary>
    /// The bot control command type for viewing the playback queue.
    /// </summary>
    public const string ViewList = "viewList";

    /// <summary>
    /// The bot control command type for clearing the playback queue.
    /// </summary>
    public const string Clear = "clear";

    /// <summary>
    /// The bot control command type for removing a queue item.
    /// </summary>
    public const string Remove = "remove";

    /// <summary>
    /// The bot control command type for shuffling the queue.
    /// </summary>
    public const string Shuffle = "shuffle";

    /// <summary>
    /// The bot control command type for moving a queue item.
    /// </summary>
    public const string MoveToIndex = "moveToIndex";

    /// <summary>
    /// The bot control command type for toggling single-track repeat.
    /// </summary>
    public const string Repeat = "repeat";

    /// <summary>
    /// The bot control command type for toggling playback batch repeat.
    /// </summary>
    public const string RepeatList = "repeatList";

    /// <summary>
    /// The bot control command type for creating a saved playlist.
    /// </summary>
    public const string CreatePlaylist = "createPlaylist";

    /// <summary>
    /// The bot control command type for saving a playlist from a URL.
    /// </summary>
    public const string SavePlaylist = "savePlaylist";

    /// <summary>
    /// The bot control command type for loading a saved playlist.
    /// </summary>
    public const string LoadPlaylist = "loadPlaylist";

    /// <summary>
    /// The bot control command type for deleting a saved playlist.
    /// </summary>
    public const string DeletePlaylist = "deletePlaylist";

    /// <summary>
    /// The bot control command type for adding a song to a saved playlist.
    /// </summary>
    public const string AddSong = "addSong";

    /// <summary>
    /// The bot control command type for removing a song from a saved playlist.
    /// </summary>
    public const string RemoveSong = "removeSong";

    /// <summary>
    /// The bot control command type for listing saved playlists.
    /// </summary>
    public const string ListPlaylists = "listPlaylists";

    /// <summary>
    /// The bot control command type for viewing a saved playlist.
    /// </summary>
    public const string ViewPlaylist = "viewPlaylist";

    /// <summary>
    /// The bot control command type for renaming a saved playlist.
    /// </summary>
    public const string RenamePlaylist = "renamePlaylist";
}
