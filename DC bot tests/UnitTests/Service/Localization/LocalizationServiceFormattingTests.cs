namespace DC_bot_tests.UnitTests.Service.Localization;

[Trait("Category", "Unit")]
public class LocalizationServiceFormattingTests : LocalizationServiceTestBase
{
    [Fact]
    public void Get_WithFormatting_ReturnsFormattedString()
    {
        const ulong guildId = 123456;
        const string key = "track_added";
        const string translationTemplate = "Added {0} by {1} to queue";
        const string trackTitle = "Song Name";
        const string artist = "Artist Name";
        SetupLocalizationDirectory();
        SetupNoGuildLanguageFile(guildId);
        SetupTranslationFile("eng", $"{{\"{key}\":\"{translationTemplate}\"}}");
        var service = CreateService();

        service.LoadLanguage(guildId);
        var result = service.Get(key, trackTitle, artist);

        Assert.Equal($"Added {trackTitle} by {artist} to queue", result);
    }
}
