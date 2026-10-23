namespace DC_bot.Service.ReactionHandler;

/// <summary>
/// Defines the emojis recognized by reaction-based music controls.
/// </summary>
internal static class ReactionControlEmojis
{
    public const string PauseEmojiName = ":pause_button:";
    public const string ResumeEmojiName = ":arrow_forward:";
    public const string SkipEmojiName = ":track_next:";
    public const string RepeatEmojiName = ":repeat:";
    public const string PauseEmoji = "\u23F8\uFE0F";
    public const string ResumeEmoji = "\u25B6\uFE0F";
    public const string SkipEmoji = "\u23ED\uFE0F";
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
