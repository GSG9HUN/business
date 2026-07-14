namespace DC_bot.Constants;

/// <summary>
///     Contains all localization keys used throughout the application.
/// </summary>
public static class LocalizationKeys
{
    // Clear Command
    public const string ClearCommandDescription = "clear_command_description";
    public const string ClearCommandResponse = "clear_command_response";
    public const string ClearCommandConfirmationRequired = "clear_command_confirmation_required";

    // Help Command
    public const string HelpCommandDescription = "help_command_description";
    public const string HelpCommandResponse = "help_command_response";

    // Join Command
    public const string JoinCommandDescription = "join_command_description";

    // Language Command
    public const string LanguageCommandDescription = "language_command_description";
    public const string LanguageCommandInvalidLanguage = "language_command_invalid_language";
    public const string LanguageCommandResponse = "language_command_response";
    public const string LanguageCommandError = "language_command_error";

    // Leave Command
    public const string LeaveCommandDescription = "leave_command_description";

    // Ping Command
    public const string PingCommandDescription = "ping_command_description";
    public const string PingCommandResponse = "ping_command_response";

    // Play Command
    public const string PlayCommandFailedToFindMusicUrlError = "play_command_failed_to_find_music_url_error";
    public const string PlayCommandMusicPlaying = "play_command_music_playing";
    public const string PlayCommandMusicAddedQueue = "play_command_music_added_queue";
    public const string PlayCommandListAddedQueue = "play_command_list_added_queue";
    public const string PlayCommandDescription = "play_command_description";

    // Playlist Commands
    public const string CreatePlaylistCommandDescription = "createPlaylist_command_description";
    public const string CreatePlaylistCommandCreated = "createPlaylist_command_created";
    public const string CreatePlaylistCommandAlreadyExists = "createPlaylist_command_already_exists";
    public const string CreatePlaylistCommandInvalidPlaylistName = "createPlaylist_command_invalid_playlist_name";
    public const string CreatePlaylistCommandUnknownError = "createPlaylist_command_unknown_error";
    public const string SavePlaylistCommandDescription = "save_playlist_command_description";
    public const string SavePlaylistCommandSaved = "save_playlist_command_saved";
    public const string SavePlaylistCommandAlreadyExists = "save_playlist_command_already_exists";
    public const string SavePlaylistCommandNoTracksFound = "save_playlist_command_no_tracks_found";
    public const string SavePlaylistCommandUnknownError = "save_playlist_command_unknown_error";
    public const string DeletePlaylistCommandDescription = "deletePlaylist_command_description";
    public const string DeletePlaylistCommandDeleted = "deletePlaylist_command_deleted";
    public const string DeletePlaylistCommandDoesNotExist = "deletePlaylist_command_does_not_exist";
    public const string DeletePlaylistCommandUnknownError = "deletePlaylist_command_unknown_error";
    public const string AddSongToPlaylistCommandDescription = "addSong_command_description";
    public const string AddSongToPlaylistCommandAdded = "addSong_command_added";
    public const string AddSongToPlaylistCommandPlaylistDoesNotExist = "addSong_command_playlist_does_not_exist";
    public const string AddSongToPlaylistCommandNoTracksFound = "addSong_command_no_tracks_found";
    public const string AddSongToPlaylistCommandInvalidSongUrl = "addSong_command_invalid_song_url";
    public const string AddSongToPlaylistCommandUnknownError = "addSong_command_unknown_error";
    public const string RemoveSongFromPlaylistCommandDescription = "removeSong_command_description";
    public const string RemoveSongFromPlaylistCommandRemoved = "removeSong_command_removed";
    public const string RemoveSongFromPlaylistCommandPlaylistDoesNotExist = "removeSong_command_playlist_does_not_exist";
    public const string RemoveSongFromPlaylistCommandSongNotFound = "removeSong_command_song_not_found";
    public const string RemoveSongFromPlaylistCommandInvalidPlaylistName = "removeSong_command_invalid_playlist_name";
    public const string RemoveSongFromPlaylistCommandInvalidTrackNumber = "removeSong_command_invalid_track_number";
    public const string RemoveSongFromPlaylistCommandUnknownError = "removeSong_command_unknown_error";
    public const string ListPlaylistsCommandDescription = "listPlaylists_command_description";
    public const string ListPlaylistsCommandResponse = "listPlaylists_command_response";
    public const string ListPlaylistsCommandItem = "listPlaylists_command_item";
    public const string ListPlaylistsCommandNoPlaylists = "listPlaylists_command_no_playlists";
    public const string ListPlaylistsCommandUnknownError = "listPlaylists_command_unknown_error";
    public const string ViewPlaylistCommandDescription = "viewPlaylist_command_description";
    public const string ViewPlaylistCommandResponse = "viewPlaylist_command_response";
    public const string ViewPlaylistCommandTrack = "viewPlaylist_command_track";
    public const string ViewPlaylistCommandMoreTracks = "viewPlaylist_command_more_tracks";
    public const string ViewPlaylistCommandPlaylistDoesNotExist = "viewPlaylist_command_playlist_does_not_exist";
    public const string ViewPlaylistCommandEmptyPlaylist = "viewPlaylist_command_empty_playlist";
    public const string ViewPlaylistCommandUnknownError = "viewPlaylist_command_unknown_error";
    public const string RenamePlaylistCommandDescription = "renamePlaylist_command_description";
    public const string RenamePlaylistCommandRenamed = "renamePlaylist_command_renamed";
    public const string RenamePlaylistCommandPlaylistDoesNotExist = "renamePlaylist_command_playlist_does_not_exist";
    public const string RenamePlaylistCommandPlaylistAlreadyExists = "renamePlaylist_command_playlist_already_exists";
    public const string RenamePlaylistCommandInvalidPlaylistName = "renamePlaylist_command_invalid_playlist_name";
    public const string RenamePlaylistCommandUnknownError = "renamePlaylist_command_unknown_error";

    // Pause Command
    public const string PauseCommandError = "pause_command_error";
    public const string PauseCommandResponse = "pause_command_response";
    public const string PauseCommandDescription = "pause_command_description";

    // Resume Command
    public const string ResumeCommandError = "resume_command_error";
    public const string ResumeCommandResponse = "resume_command_response";
    public const string ResumeCommandDescription = "resume_command_description";

    // Skip Command
    public const string SkipCommandError = "skip_command_error";
    public const string SkipCommandQueueIsEmpty = "skip_command_queue_is_empty";
    public const string SkipCommandDescription = "skip_command_description";

    // Shuffle Command
    public const string ShuffleCommandDescription = "shuffle_command_description";
    public const string ShuffleCommandError = "shuffle_command_error";
    public const string ShuffleCommandNotEnoughTracks = "shuffle_command_not_enough_tracks";
    public const string ShuffleCommandResponse = "shuffle_command_response";

    // Repeat Command
    public const string RepeatCommandDescription = "repeat_command_description";
    public const string RepeatCommandRepeatingOn = "repeat_command_repeating_on";
    public const string RepeatCommandRepeatingOff = "repeat_command_repeating_off";
    public const string RepeatCommandListAlreadyRepeating = "repeat_command_list_already_repeating";
    public const string RepeatListCommandTrackAlreadyRepeating = "repeat_list_command_track_already_repeating";

    // RepeatList Command
    public const string RepeatListCommandDescription = "repeat_list_command_description";
    public const string RepeatListCommandRepeatingOn = "repeat_list_command_repeating_on";
    public const string RepeatListCommandRepeatingOff = "repeat_list_command_repeating_off";

    // Tag Command
    public const string TagCommandDescription = "tag_command_description";
    public const string TagCommandResponse = "tag_command_response";
    public const string TagCommandUserNotExistError = "tag_command_user_not_exist_error";

    // ViewQueue Command
    public const string ViewListCommandDescription = "view_list_command_description";
    public const string ViewListCommandEmbedTitle = "view_list_command_embed_title";
    public const string ViewListCommandResponse = "view_list_command_response";
    public const string ViewListCommandError = "view_list_command_error";

    // Reaction Handler
    public const string MusicControl = "music_control";
    public const string PauseReaction = "pause";
    public const string ResumeReaction = "resume";
    public const string SkipReaction = "skip";
    public const string RepeatReaction = "repeat";
    public const string ReactionHandlerRepeatOn = "reaction_handler_repeat_on";
    public const string ReactionHandlerRepeatOff = "reaction_handler_repeat_off";

    // Unknown Command
    public const string UnknownCommandError = "unknown_command_error";

    // Slash Commands
    public const string SlashCommandGuildOnly = "slash_command_guild_only";
    public const string SlashCommandDeferredAccepted = "slash_command_deferred_accepted";
    public const string SlashCommandNotRegistered = "slash_command_not_registered";
    public const string SlashCommandUnexpectedError = "slash_command_unexpected_error";
}

/// <summary>
///     Contains all validation error keys used for user and system validation.
/// </summary>
public static class ValidationErrorKeys
{
    public const string UserNotInVoiceChannel = "user_not_in_a_voice_channel";
    public const string LavalinkError = "lavalink_error";
    public const string BotIsNotConnectedError = "bot_is_not_connected_error";
}
