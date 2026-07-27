using DC_bot.Interface.Service.IO;
using DC_bot.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Localization;

public abstract class LocalizationServiceTestBase
{
    protected Mock<IFileSystem> FileSystemMock { get; } = new();

    protected LocalizationService CreateService(ILogger<LocalizationService>? logger = null)
    {
        return new LocalizationService(logger ?? NullLogger<LocalizationService>.Instance, FileSystemMock.Object);
    }

    protected void SetupLocalizationDirectory(bool exists = true)
    {
        FileSystemMock
            .Setup(x => x.DirectoryExists(It.IsAny<string>()))
            .Returns(exists);
    }

    protected void SetupNoGuildLanguageFile(ulong guildId)
    {
        FileSystemMock
            .Setup(x => x.FileExists(It.Is<string>(p => p.Contains("guildFiles") && p.Contains($"{guildId}.json"))))
            .Returns(false);
    }

    protected void SetupGuildLanguageFile(ulong guildId, string languageCode)
    {
        FileSystemMock
            .Setup(x => x.FileExists(It.Is<string>(p => p.Contains("guildFiles") && p.Contains($"{guildId}.json"))))
            .Returns(true);
        FileSystemMock
            .Setup(x => x.ReadAllText(It.Is<string>(p => p.Contains("guildFiles") && p.Contains($"{guildId}.json"))))
            .Returns($@"""{languageCode}""");
    }

    protected void SetupTranslationFile(string languageCode, string json)
    {
        FileSystemMock
            .Setup(x => x.FileExists(It.Is<string>(p => p.Contains("localization") && p.EndsWith($"{languageCode}.json"))))
            .Returns(true);
        FileSystemMock
            .Setup(x => x.ReadAllText(It.Is<string>(p => p.EndsWith($"{languageCode}.json"))))
            .Returns(json);
    }
}
