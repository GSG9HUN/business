using DC_bot.Exceptions.Localization;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Localization;

[Trait("Category", "Unit")]
public class LocalizationServiceGuildLanguageTests : LocalizationServiceTestBase
{
    [Fact]
    public void LocalizationService_LoadAndSaveLanguage_Works()
    {
        const ulong guildId = 987654321;
        const string languageCode = "hu";
        SetupLocalizationDirectory();
        FileSystemMock.Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>())).Callback(() => { });
        SetupGuildLanguageFile(guildId, languageCode);
        SetupTranslationFile(languageCode, "{\"play_command\":\"Play Command\",\"pause_command\":\"Pause Command\"}");
        var service = CreateService();

        service.SaveLanguage(guildId, languageCode);
        service.LoadLanguage(guildId);

        Assert.Equal("Play Command", service.Get(guildId, "play_command"));
        Assert.Equal("Pause Command", service.Get(guildId, "pause_command"));
    }

    [Fact]
    public void Get_WithMultipleLoadedGuilds_ReturnsEachGuildsOwnLanguage()
    {
        const ulong hungarianGuildId = 111;
        const ulong englishGuildId = 222;
        const string key = "greeting";
        SetupLocalizationDirectory();
        SetupGuildLanguageFile(hungarianGuildId, "hu");
        SetupGuildLanguageFile(englishGuildId, "eng");
        SetupTranslationFile("hu", $"{{\"{key}\":\"Szia\"}}");
        SetupTranslationFile("eng", $"{{\"{key}\":\"Hello\"}}");
        var service = CreateService();

        service.LoadLanguage(hungarianGuildId);
        service.LoadLanguage(englishGuildId);

        Assert.Equal("Szia", service.Get(hungarianGuildId, key));
        Assert.Equal("Hello", service.Get(englishGuildId, key));
    }

    [Fact]
    public void LoadLanguage_GuildLanguageFileExists_LoadsGuildLanguage()
    {
        const ulong guildId = 123456789;
        SetupLocalizationDirectory();
        FileSystemMock
            .Setup(x => x.FileExists(It.IsAny<string>()))
            .Returns(true);
        FileSystemMock
            .Setup(x => x.ReadAllText(It.Is<string>(s => s.Contains($"{guildId}.json"))))
            .Returns(@"""hu""");
        FileSystemMock
            .Setup(x => x.ReadAllText(It.Is<string>(s => s.Contains("hu.json"))))
            .Returns(@"{ ""test_key"": ""Hungarian Value"" }");
        var service = CreateService();

        service.LoadLanguage(guildId);

        Assert.Equal("Hungarian Value", service.Get(guildId, "test_key"));
    }

    [Fact]
    public void SaveLanguage_ValidLanguageCode_SavesGuildLanguage()
    {
        const ulong guildId = 123456789;
        const string languageCode = "hu";
        SetupLocalizationDirectory();
        FileSystemMock.Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>())).Callback(() => { });
        FileSystemMock
            .Setup(x => x.FileExists(It.IsAny<string>()))
            .Returns(true);
        FileSystemMock
            .Setup(x => x.ReadAllText(It.Is<string>(s => s.Contains("hu.json"))))
            .Returns(@"{ ""test_key"": ""Hungarian Value"" }");
        var service = CreateService();

        service.SaveLanguage(guildId, languageCode);

        FileSystemMock.Verify(
            x => x.WriteAllText(It.Is<string>(s => s.Contains($"{guildId}.json")), It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public void SaveLanguage_InvalidLanguageFile_ThrowsLocalizationException()
    {
        SetupLocalizationDirectory();
        FileSystemMock.Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>())).Callback(() => { });
        FileSystemMock
            .Setup(x => x.FileExists(It.IsAny<string>()))
            .Returns(false);
        var service = CreateService();

        Assert.Throws<LocalizationException>(() => service.SaveLanguage(123456789, "invalid"));
    }
}
