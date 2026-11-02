namespace DC_bot.Service.ReactionHandler;

/// <summary>
/// Defines the emojis recognized by reaction-based music controls.
/// </summary>
internal static class ReactionControlEmojis
{

    /// <summary>Discord reaction name for pause playback control.</summary>
    public const string PauseEmojiName = ":pause_button:";

    /// <summary>Discord reaction name for resume playback control.</summary>
    public const string ResumeEmojiName = ":arrow_forward:";

    /// <summary>Discord reaction name for skip playback control.</summary>
    public const string SkipEmojiName = ":track_next:";

    /// <summary>Discord reaction name for repeat playback control.</summary>
    public const string RepeatEmojiName = ":repeat:";

    /// <summary>Discord emoji value for pause playback control.</summary>
    public const string PauseEmoji = "\u23F8\uFE0F";

    /// <summary>Discord emoji value for resume playback control.</summary>
    public const string ResumeEmoji = "\u25B6\uFE0F";

    /// <summary>Discord emoji value for skip playback control.</summary>
    public const string SkipEmoji = "\u23ED\uFE0F";

    /// <summary>Discord emoji value for repeat playback control.</summary>
    public const string RepeatEmoji = "\uD83D\uDD01";

    /// <summary>
    /// Normalizes supported input before provider-specific validation or dispatch.
    /// </summary>
    /// <param name="emojiName">The emoji name supplied for this operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    public static string Normalize(string emojiName)
    {
        return emojiName switch
        {
            PauseEmoji => PauseEmojiName,
            ResumeEmoji => ResumeEmojiName,
            SkipEmoji => SkipEmojiName,
            RepeatEmoji => RepeatEmojiName,
            _ => emojiName
        };
    }
}
