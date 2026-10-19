namespace DC_bot.Constants;

/// <summary>
///     Contains all validation error keys used for user and system validation.
/// </summary>
public static class ValidationErrorKeys
{
    public const string UserNotInVoiceChannel = "user_not_in_a_voice_channel";
    public const string LavalinkError = "lavalink_error";
    public const string BotIsNotConnectedError = "bot_is_not_connected_error";
    public const string PlaybackStatePending = "playback_state_pending";
    public const string PlaybackOutcomeUnknown = "playback_outcome_unknown";
    public const string PlaybackSuperseded = "playback_superseded";
    public const string PlaybackBusy = "playback_busy";
}
