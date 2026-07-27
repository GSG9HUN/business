using DC_bot.Exceptions.Localization;
using DC_bot.Service;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Localization;

[Trait("Category", "Unit")]
public class LocalizationServiceErrorHandlingTests : LocalizationServiceTestBase
{
    [Fact]
    public void SaveLanguage_WriteFails_ThrowsLocalizationException()
    {
        const ulong guildId = 123456789;
        const string languageCode = "hu";
        SetupLocalizationDirectory();
        FileSystemMock.Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new IOException("Disk error"));
        FileSystemMock
            .Setup(x => x.FileExists(It.IsAny<string>()))
            .Returns(true);
        FileSystemMock
            .Setup(x => x.ReadAllText(It.Is<string>(s => s.Contains("hu.json"))))
            .Returns(@"{ ""test_key"": ""Value"" }");
        var service = CreateService();

        Assert.Throws<LocalizationException>(() => service.SaveLanguage(guildId, languageCode));
    }

    [Fact]
    public void ReadJson_TranslationFileReadFails_ThrowsLocalizationExceptionAndLogsError()
    {
        var loggerMock = CreateLoggerMock();
        SetupLocalizationDirectory();
        FileSystemMock
            .Setup(x => x.FileExists(It.Is<string>(p => p.Contains("guildFiles"))))
            .Returns(false);
        FileSystemMock
            .Setup(x => x.FileExists(It.Is<string>(p => p.EndsWith("eng.json"))))
            .Returns(true);
        FileSystemMock
            .Setup(x => x.ReadAllText(It.Is<string>(p => p.EndsWith("eng.json"))))
            .Throws(new IOException("Disk read error"));
        var service = CreateService(loggerMock.Object);

        var ex = Assert.Throws<LocalizationException>(() => service.LoadLanguage(123456));

        Assert.Contains("Failed to read JSON file", ex.Message);
        Assert.IsType<IOException>(ex.InnerException);
        VerifyReadFailedLog(loggerMock);
    }

    [Fact]
    public void ReadJson_GuildLanguageFileReadFails_ThrowsLocalizationExceptionAndLogsError()
    {
        const ulong guildId = 123456789;
        var loggerMock = CreateLoggerMock();
        SetupLocalizationDirectory();
        FileSystemMock
            .Setup(x => x.FileExists(It.IsAny<string>()))
            .Returns(true);
        FileSystemMock
            .Setup(x => x.ReadAllText(It.Is<string>(p => p.Contains($"{guildId}.json"))))
            .Throws(new UnauthorizedAccessException("Access denied"));
        var service = CreateService(loggerMock.Object);

        var ex = Assert.Throws<LocalizationException>(() => service.LoadLanguage(guildId));

        Assert.Contains("Failed to read JSON file", ex.Message);
        Assert.IsType<UnauthorizedAccessException>(ex.InnerException);
        VerifyReadFailedLog(loggerMock);
    }

    [Fact]
    public void ReadJson_InvalidJson_ThrowsLocalizationExceptionAndLogsError()
    {
        var loggerMock = CreateLoggerMock();
        SetupLocalizationDirectory();
        FileSystemMock
            .Setup(x => x.FileExists(It.Is<string>(p => p.Contains("guildFiles"))))
            .Returns(false);
        FileSystemMock
            .Setup(x => x.FileExists(It.Is<string>(p => p.EndsWith("eng.json"))))
            .Returns(true);
        FileSystemMock
            .Setup(x => x.ReadAllText(It.Is<string>(p => p.EndsWith("eng.json"))))
            .Returns("{ invalid json content }}}");
        var service = CreateService(loggerMock.Object);

        var ex = Assert.Throws<LocalizationException>(() => service.LoadLanguage(123456));

        Assert.Contains("Failed to read JSON file", ex.Message);
        VerifyReadFailedLog(loggerMock);
    }

    private static Mock<ILogger<LocalizationService>> CreateLoggerMock()
    {
        var loggerMock = new Mock<ILogger<LocalizationService>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        return loggerMock;
    }

    private static void VerifyReadFailedLog(Mock<ILogger<LocalizationService>> loggerMock)
    {
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Localization read failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
