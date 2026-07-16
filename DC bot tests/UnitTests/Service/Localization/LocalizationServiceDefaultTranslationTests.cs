using System.Text.Json;
using DC_bot.Exceptions.Localization;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Localization;

[Trait("Category", "Unit")]
public class LocalizationServiceDefaultTranslationTests : LocalizationServiceTestBase
{
    [Fact]
    public void Get_KeyExists_ReturnsTranslation()
    {
        const ulong guildId = 123456;
        const string key = "play_command_description";
        const string expectedValue = "Play a song";
        SetupLocalizationDirectory();
        SetupNoGuildLanguageFile(guildId);
        SetupTranslationFile("eng", $"{{\"{key}\":\"{expectedValue}\"}}");
        var service = CreateService();

        service.LoadLanguage(guildId);
        var result = service.Get(guildId, key);

        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void Get_KeyDoesNotExist_ReturnsKeyItself()
    {
        const ulong guildId = 123456;
        const string key = "nonexistent_key";
        SetupLocalizationDirectory();
        SetupNoGuildLanguageFile(guildId);
        SetupTranslationFile("eng", "{}");
        var service = CreateService();

        service.LoadLanguage(guildId);
        var result = service.Get(key);

        Assert.Equal(key, result);
    }

    [Fact]
    public void Get_MultipleKeys_ReturnsAllTranslations()
    {
        const ulong guildId = 123456;
        var keys = new Dictionary<string, string>
        {
            { "command1", "Translation 1" },
            { "command2", "Translation 2" },
            { "command3", "Translation 3" }
        };
        SetupLocalizationDirectory();
        SetupNoGuildLanguageFile(guildId);
        SetupTranslationFile("eng", JsonSerializer.Serialize(keys));
        var service = CreateService();

        service.LoadLanguage(guildId);

        foreach (var kvp in keys)
        {
            var result = service.Get(guildId, kvp.Key);
            Assert.Equal(kvp.Value, result);
        }
    }

    [Fact]
    public void LoadLanguage_GuildLanguageFileDoesNotExist_LoadsDefaultLanguage()
    {
        const ulong guildId = 123456789;
        SetupLocalizationDirectory();
        SetupNoGuildLanguageFile(guildId);
        SetupTranslationFile("eng", "{\"test_key\":\"English Value\"}");
        var service = CreateService();

        service.LoadLanguage(guildId);

        var result = service.Get("test_key");
        Assert.Equal("English Value", result);
    }

    [Fact]
    public void LoadLanguage_TranslationFileNotFound_ThrowsLocalizationException()
    {
        const ulong guildId = 123456789;
        SetupLocalizationDirectory();
        FileSystemMock
            .Setup(x => x.FileExists(It.IsAny<string>()))
            .Returns(false);
        var service = CreateService();

        Assert.Throws<LocalizationException>(() => service.LoadLanguage(guildId));
    }
}
