using DC_bot.Exceptions.Localization;
using DC_bot.Interface.Service.IO;
using DC_bot.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Localization;

public class LocalizationServiceTests
{
    private readonly Mock<IFileSystem> _mockFileSystem = new();

    #region Constructor Tests

    [Fact]
    public void Constructor_CreatesLocalizationDirectory_WhenDirectoryDoesNotExist()
    {
        // Arrange
        _mockFileSystem
            .Setup(x => x.DirectoryExists(It.IsAny<string>()))
            .Returns(false);

        // Act
        var service = new LocalizationService(NullLogger<LocalizationService>.Instance, _mockFileSystem.Object);

        // Assert
        _mockFileSystem.Verify(x => x.CreateDirectory(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void Constructor_DoesNotCreateDirectory_WhenDirectoryExists()
    {
        // Arrange
        _mockFileSystem
            .Setup(x => x.DirectoryExists(It.IsAny<string>()))
            .Returns(true);

        // Act
        var service = new LocalizationService(NullLogger<LocalizationService>.Instance, _mockFileSystem.Object);

        // Assert
        _mockFileSystem.Verify(x => x.CreateDirectory(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Get Tests

    [Fact]
    public void Get_KeyExists_ReturnsTranslation()
    {
        // Arrange
        const string key = "play_command_description";
        const string expectedValue = "Play a song";

        _mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);

        _mockFileSystem
            .Setup(x => x.FileExists(It.Is<string>(p => p.Contains("guildFiles") && p.Contains("123456.json"))))
            .Returns(false);

        _mockFileSystem
            .Setup(x => x.FileExists(It.Is<string>(p => p.Contains("localization") && p.EndsWith("eng.json"))))
            .Returns(true);

        var json = $"{{\"{key}\":\"{expectedValue}\"}}";

        _mockFileSystem
            .Setup(x => x.ReadAllText(It.Is<string>(p => p.EndsWith("eng.json"))))
            .Returns(json);

        var service = new LocalizationService(NullLogger<LocalizationService>.Instance, _mockFileSystem.Object);

        // Act
        service.LoadLanguage(123456);
        var result = service.Get(key);

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void Get_KeyDoesNotExist_ReturnsKeyItself()
    {
        // Arrange
        const string key = "nonexistent_key";

        _mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);

        // Guild preference file missing -> service falls back to default eng.json translations.
        _mockFileSystem
            .Setup(x => x.FileExists(It.Is<string>(p => p.Contains("guildFiles") && p.Contains("123456.json"))))
            .Returns(false);

        _mockFileSystem
            .Setup(x => x.FileExists(It.Is<string>(p => p.Contains("localization") && p.EndsWith("eng.json"))))
            .Returns(true);

        _mockFileSystem
            .Setup(x => x.ReadAllText(It.Is<string>(p => p.EndsWith("eng.json"))))
            .Returns("{}");

        var service = new LocalizationService(NullLogger<LocalizationService>.Instance, _mockFileSystem.Object);
        service.LoadLanguage(123456);

        // Act
        var result = service.Get(key);

        // Assert
        Assert.Equal(key, result);
    }

    [Fact]
    public void Get_WithFormatting_ReturnsFormattedString()
    {
        // Arrange
        const string key = "track_added";
        const string translationTemplate = "Added {0} by {1} to queue";
        const string trackTitle = "Song Name";
        const string artist = "Artist Name";
        var expectedResult = $"Added {trackTitle} by {artist} to queue";

        _mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);

        _mockFileSystem
            .Setup(x => x.FileExists(It.Is<string>(p => p.Contains("guildFiles") && p.Contains("123456.json"))))
            .Returns(false);

        _mockFileSystem
            .Setup(x => x.FileExists(It.Is<string>(p => p.Contains("localization") && p.EndsWith("eng.json"))))
            .Returns(true);

        var json = $"{{\"{key}\":\"{translationTemplate}\"}}";

        _mockFileSystem
            .Setup(x => x.ReadAllText(It.Is<string>(p => p.EndsWith("eng.json"))))
            .Returns(json);

        var service = new LocalizationService(NullLogger<LocalizationService>.Instance, _mockFileSystem.Object);
        service.LoadLanguage(123456);

        // Act
        var result = service.Get(key, trackTitle, artist);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void Get_MultipleKeys_ReturnsAllTranslations()
    {
        // Arrange
        var keys = new Dictionary<string, string>
        {
            { "command1", "Translation 1" },
            { "command2", "Translation 2" },
            { "command3", "Translation 3" }
        };

        _mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);

        _mockFileSystem
            .Setup(x => x.FileExists(It.Is<string>(p => p.Contains("guildFiles") && p.Contains("123456.json"))))
            .Returns(false);

        _mockFileSystem
            .Setup(x => x.FileExists(It.Is<string>(p => p.Contains("localization") && p.EndsWith("eng.json"))))
            .Returns(true);

        var json = System.Text.Json.JsonSerializer.Serialize(keys);

        _mockFileSystem
            .Setup(x => x.ReadAllText(It.Is<string>(p => p.EndsWith("eng.json"))))
            .Returns(json);

        var service = new LocalizationService(NullLogger<LocalizationService>.Instance, _mockFileSystem.Object);
        service.LoadLanguage(123456);

        // Act & Assert
        foreach (var kvp in keys)
        {
            var result = service.Get(kvp.Key);
            Assert.Equal(kvp.Value, result);
        }
    }

    #endregion

    #region LoadLanguage Tests

    [Fact]
    public void LoadLanguage_GuildLanguageFileExists_LoadsGuildLanguage()
    {
        // Arrange
        const ulong guildId = 123456789;
        const string guildLanguage = "hu";

        _mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
        _mockFileSystem
            .Setup(x => x.FileExists(It.IsAny<string>()))
            .Returns(true);

        _mockFileSystem
            .Setup(x => x.ReadAllText(It.Is<string>(s => s.Contains($"{guildId}.json"))))
            .Returns($@"""{guildLanguage}""");

        _mockFileSystem
            .Setup(x => x.ReadAllText(It.Is<string>(s => s.Contains("hu.json"))))
            .Returns(@"{ ""test_key"": ""Hungarian Value"" }");

        var service = new LocalizationService(NullLogger<LocalizationService>.Instance, _mockFileSystem.Object);

        // Act
        service.LoadLanguage(guildId);

        // Assert
        var result = service.Get("test_key");
        Assert.Equal("Hungarian Value", result);
    }

    [Fact]
    public void LoadLanguage_GuildLanguageFileDoesNotExist_LoadsDefaultLanguage()
    {
        // Arrange
        const ulong guildId = 123456789;

        _mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);

        _mockFileSystem
            .Setup(x => x.FileExists(It.Is<string>(p => p.Contains("guildFiles") && p.Contains($"{guildId}.json"))))
            .Returns(false);

        _mockFileSystem
            .Setup(x => x.FileExists(It.Is<string>(p => p.Contains("localization") && p.EndsWith("eng.json"))))
            .Returns(true);

        var englishJson = "{\"test_key\":\"English Value\"}";
        _mockFileSystem
            .Setup(x => x.ReadAllText(It.Is<string>(s => s.EndsWith("eng.json"))))
            .Returns(englishJson);

        var service = new LocalizationService(NullLogger<LocalizationService>.Instance, _mockFileSystem.Object);

        // Act
        service.LoadLanguage(guildId);

        // Assert
        var result = service.Get("test_key");
        Assert.Equal("English Value", result);
    }

    [Fact]
    public void LoadLanguage_TranslationFileNotFound_ThrowsLocalizationException()
    {
        // Arrange
        const ulong guildId = 123456789;

        _mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
        _mockFileSystem
            .Setup(x => x.FileExists(It.IsAny<string>()))
            .Returns(false); // No files exist

        var service = new LocalizationService(NullLogger<LocalizationService>.Instance, _mockFileSystem.Object);

        // Act & Assert
        Assert.Throws<LocalizationException>(() => service.LoadLanguage(guildId));
    }

    #endregion

    #region SaveLanguage Tests

    [Fact]
    public void SaveLanguage_ValidLanguageCode_SavesGuildLanguage()
    {
        // Arrange
        const ulong guildId = 123456789;
        const string languageCode = "hu";

        _mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
        _mockFileSystem.Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>())).Callback(() => { });
        _mockFileSystem
            .Setup(x => x.FileExists(It.IsAny<string>()))
            .Returns(true);

        var languageJson = @"{ ""test_key"": ""Hungarian Value"" }";
        _mockFileSystem
            .Setup(x => x.ReadAllText(It.Is<string>(s => s.Contains("hu.json"))))
            .Returns(languageJson);

        var service = new LocalizationService(NullLogger<LocalizationService>.Instance, _mockFileSystem.Object);

        // Act
        service.SaveLanguage(guildId, languageCode);

        // Assert
        _mockFileSystem.Verify(
            x => x.WriteAllText(It.Is<string>(s => s.Contains($"{guildId}.json")), It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public void SaveLanguage_InvalidLanguageFile_ThrowsLocalizationException()
    {
        // Arrange
        const ulong guildId = 123456789;
        const string languageCode = "invalid";

        _mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
        _mockFileSystem.Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>())).Callback(() => { });
        _mockFileSystem
            .Setup(x => x.FileExists(It.IsAny<string>()))
            .Returns(false); // Language file doesn't exist

        var service = new LocalizationService(NullLogger<LocalizationService>.Instance, _mockFileSystem.Object);

        // Act & Assert
        Assert.Throws<LocalizationException>(() => service.SaveLanguage(guildId, languageCode));
    }

    [Fact]
    public void SaveLanguage_WriteFails_ThrowsLocalizationException()
    {
        // Arrange
        const ulong guildId = 123456789;
        const string languageCode = "hu";

        _mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
        _mockFileSystem.Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new IOException("Disk error"));
        _mockFileSystem
            .Setup(x => x.FileExists(It.IsAny<string>()))
            .Returns(true);

        var languageJson = @"{ ""test_key"": ""Value"" }";
        _mockFileSystem
            .Setup(x => x.ReadAllText(It.Is<string>(s => s.Contains("hu.json"))))
            .Returns(languageJson);

        var service = new LocalizationService(NullLogger<LocalizationService>.Instance, _mockFileSystem.Object);

        // Act & Assert
        Assert.Throws<LocalizationException>(() => service.SaveLanguage(guildId, languageCode));
    }

    #endregion

    #region ReadJson Exception Tests

    [Fact]
    public void ReadJson_TranslationFileReadFails_ThrowsLocalizationExceptionAndLogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LocalizationService>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        _mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);

        _mockFileSystem
            .Setup(x => x.FileExists(It.Is<string>(p => p.Contains("guildFiles"))))
            .Returns(false);

        _mockFileSystem
            .Setup(x => x.FileExists(It.Is<string>(p => p.EndsWith("eng.json"))))
            .Returns(true);

        _mockFileSystem
            .Setup(x => x.ReadAllText(It.Is<string>(p => p.EndsWith("eng.json"))))
            .Throws(new IOException("Disk read error"));

        var service = new LocalizationService(loggerMock.Object, _mockFileSystem.Object);

        // Act & Assert
        var ex = Assert.Throws<LocalizationException>(() => service.LoadLanguage(123456));
        Assert.Contains("Failed to read JSON file", ex.Message);
        Assert.IsType<IOException>(ex.InnerException);

        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Localization read failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void ReadJson_GuildLanguageFileReadFails_ThrowsLocalizationExceptionAndLogsError()
    {
        // Arrange
        const ulong guildId = 123456789;
        var loggerMock = new Mock<ILogger<LocalizationService>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        _mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);

        _mockFileSystem
            .Setup(x => x.FileExists(It.IsAny<string>()))
            .Returns(true);

        _mockFileSystem
            .Setup(x => x.ReadAllText(It.Is<string>(p => p.Contains($"{guildId}.json"))))
            .Throws(new UnauthorizedAccessException("Access denied"));

        var service = new LocalizationService(loggerMock.Object, _mockFileSystem.Object);

        // Act & Assert
        var ex = Assert.Throws<LocalizationException>(() => service.LoadLanguage(guildId));
        Assert.Contains("Failed to read JSON file", ex.Message);
        Assert.IsType<UnauthorizedAccessException>(ex.InnerException);

        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Localization read failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void ReadJson_InvalidJson_ThrowsLocalizationExceptionAndLogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LocalizationService>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        _mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);

        _mockFileSystem
            .Setup(x => x.FileExists(It.Is<string>(p => p.Contains("guildFiles"))))
            .Returns(false);

        _mockFileSystem
            .Setup(x => x.FileExists(It.Is<string>(p => p.EndsWith("eng.json"))))
            .Returns(true);

        _mockFileSystem
            .Setup(x => x.ReadAllText(It.Is<string>(p => p.EndsWith("eng.json"))))
            .Returns("{ invalid json content }}}");

        var service = new LocalizationService(loggerMock.Object, _mockFileSystem.Object);

        // Act & Assert
        var ex = Assert.Throws<LocalizationException>(() => service.LoadLanguage(123456));
        Assert.Contains("Failed to read JSON file", ex.Message);

        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Localization read failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void LocalizationService_LoadAndSaveLanguage_Works()
    {
        // Arrange
        const ulong guildId = 987654321;
        const string languageCode = "hu";

        _mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
        _mockFileSystem.Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>())).Callback(() => { });

        _mockFileSystem
            .Setup(x => x.FileExists(It.Is<string>(p => p.Contains("guildFiles") && p.Contains($"{guildId}.json"))))
            .Returns(true);

        _mockFileSystem
            .Setup(x => x.FileExists(It.Is<string>(p => p.Contains("localization") && p.EndsWith("hu.json"))))
            .Returns(true);

        // Guild preference file stores a JSON string, not an object.
        _mockFileSystem
            .Setup(x => x.ReadAllText(It.Is<string>(s => s.Contains("guildFiles") && s.Contains($"{guildId}.json"))))
            .Returns("\"hu\"");

        var languageJson = "{\"play_command\":\"Play Command\",\"pause_command\":\"Pause Command\"}";
        _mockFileSystem
            .Setup(x => x.ReadAllText(It.Is<string>(s => s.EndsWith("hu.json"))))
            .Returns(languageJson);

        var service = new LocalizationService(NullLogger<LocalizationService>.Instance, _mockFileSystem.Object);

        // Act
        service.SaveLanguage(guildId, languageCode);
        service.LoadLanguage(guildId);

        var result1 = service.Get("play_command");
        var result2 = service.Get("pause_command");

        // Assert
        Assert.Equal("Play Command", result1);
        Assert.Equal("Pause Command", result2);
    }

    #endregion
}