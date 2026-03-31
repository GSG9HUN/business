namespace DC_bot.Constants;

/// <summary>
///     Contains all localization keys used throughout the application.
/// </summary>
public static class LocalizationKeys
{
    // Clear Command
    public const string ClearCommandDescription = "clear_command_description";
    public const string ClearCommandResponse = "clear_command_response";

    // Help Command
    public const string HelpCommandDescription = "help_command_description";
    public const string HelpCommandResponse = "help_command_response";

    // Join Command
    public const string JoinCommandDescription = "join_command_description";

    // Language Command
    public const string LanguageCommandDescription = "language_command_description";

    // Leave Command
    public const string LeaveCommandDescription = "leave_command_description";

    // Ping Command
    public const string PingCommandDescription = "ping_command_description";
    public const string PingCommandResponse = "Pong!";

    // Play Command
    public const string PlayCommandFailedToFindMusicUrlError = "play_command_failed_to_find_music_url_error";
    public const string PlayCommandMusicPlaying = "play_command_music_playing";
    public const string PlayCommandMusicAddedQueue = "play_command_music_added_queue";
    public const string PlayCommandListAddedQueue = "play_command_list_added_queue";
    public const string PlayCommandDescription = "play_command_description";

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
    public const string SkipCommandResponse = "skip_command_response";
    public const string SkipCommandQueueIsEmpty = "skip_command_queue_is_empty";
    public const string SkipCommandDescription = "skip_command_description";

    // Shuffle Command
    public const string ShuffleCommandDescription = "shuffle_command_description";
    public const string ShuffleCommandError = "shuffle_command_error";
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