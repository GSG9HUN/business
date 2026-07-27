using DC_bot.Service.ReactionHandler;

namespace DC_bot_tests.UnitTests.Service.ReactionHandler;

[Trait("Category", "Unit")]
public class ReactionControlEmojisTests
{
    public static IEnumerable<object[]> UnicodeEmojiCases()
    {
        yield return [ReactionControlEmojis.PauseEmoji, ReactionControlEmojis.PauseEmojiName];
        yield return [ReactionControlEmojis.ResumeEmoji, ReactionControlEmojis.ResumeEmojiName];
        yield return [ReactionControlEmojis.SkipEmoji, ReactionControlEmojis.SkipEmojiName];
        yield return [ReactionControlEmojis.RepeatEmoji, ReactionControlEmojis.RepeatEmojiName];
    }

    [Theory]
    [MemberData(nameof(UnicodeEmojiCases))]
    public void Normalize_WhenUnicodeControlEmoji_ReturnsDiscordEmojiName(string emoji, string expectedName)
    {
        var normalized = ReactionControlEmojis.Normalize(emoji);

        Assert.Equal(expectedName, normalized);
    }

    [Theory]
    [InlineData(ReactionControlEmojis.PauseEmojiName)]
    [InlineData(ReactionControlEmojis.ResumeEmojiName)]
    [InlineData(ReactionControlEmojis.SkipEmojiName)]
    [InlineData(ReactionControlEmojis.RepeatEmojiName)]
    public void Normalize_WhenAlreadyDiscordEmojiName_ReturnsInput(string emojiName)
    {
        var normalized = ReactionControlEmojis.Normalize(emojiName);

        Assert.Equal(emojiName, normalized);
    }

    [Fact]
    public void Normalize_WhenUnsupportedEmoji_ReturnsInput()
    {
        const string unsupportedEmoji = ":not_a_control:";

        var normalized = ReactionControlEmojis.Normalize(unsupportedEmoji);

        Assert.Equal(unsupportedEmoji, normalized);
    }
}
