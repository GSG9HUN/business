using System.Text.Json;
using DC_bot.Service;
using DC_bot_tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.ServiceTest;

public class LocalizationServiceTest
{
    private const string LanguageCodeEng = "eng";
    private const string TestKeyHello = "Hello";
    private const string TestValueHelloWorld = "Hello World";
    private const string TestValueHelloGuild = "Hello Guild";
    private const string TestKeyMissing = "MissingKey";
    private const ulong TestGuildId = 12345;
    private const string LocalizationRoot = "mem/localization";
    private const string TranslationRoot = "mem/translations";
    
    private readonly Mock<ILogger<LocalizationService>> _loggerMock;
    private readonly InMemoryFileSystem _fileSystem;

    public LocalizationServiceTest()
    {
        _loggerMock = new Mock<ILogger<LocalizationService>>();
        _fileSystem = new InMemoryFileSystem();

        LocalizationService.LocalizationDirectory = LocalizationRoot;
        LocalizationService.TranslationDirectory = TranslationRoot;

        _fileSystem.CreateDirectory(LocalizationRoot);
        _fileSystem.CreateDirectory(TranslationRoot);
    }

    [Fact]
    public void Constructor_CreatesLocalizationDirectory_WhenNotExists()
    {
        // Arrange
        var tempLocalizationDirectory = "mem/localization-temp";
        LocalizationService.LocalizationDirectory = tempLocalizationDirectory;
        // Act
        var service = new LocalizationService(_loggerMock.Object, _fileSystem);

        // Assert
        Assert.True(_fileSystem.DirectoryExists(tempLocalizationDirectory));
    }

    [Fact]
    public void LoadTranslations_ThrowsFileNotFoundException_WhenLanguageFileMissing()
    {
        // Arrange
        var service = new LocalizationService(_loggerMock.Object, _fileSystem);
        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => service.LoadLanguage(TestGuildId));
    }

    [Fact]
    public void Get_ReturnsKey_WhenTranslationMissing()
    {
        // Arrange
        var service = new LocalizationService(_loggerMock.Object, _fileSystem);
        CreateTranslationFile(LanguageCodeEng, new Dictionary<string, string> { { TestKeyHello, TestValueHelloWorld } });
        service.LoadLanguage(TestGuildId);

        // Act
        var result = service.Get(TestKeyMissing);

        // Assert
        Assert.Equal(TestKeyMissing, result);
    }

    [Fact]
    public void Get_ReturnsTranslatedValue_WhenKeyExists()
    {
        // Arrange
        var service = new LocalizationService(_loggerMock.Object, _fileSystem);
        CreateTranslationFile(LanguageCodeEng, new Dictionary<string, string> { { TestKeyHello, TestValueHelloWorld } });
        service.LoadLanguage(TestGuildId);

        // Act
        var result = service.Get(TestKeyHello);

        // Assert
        Assert.Equal(TestValueHelloWorld, result);
    }

    [Fact]
    public void SaveLanguage_CreatesGuildSpecificFile()
    {
        // Arrange
        var service = new LocalizationService(_loggerMock.Object, _fileSystem);

        // Act
        service.SaveLanguage(TestGuildId, LanguageCodeEng);

        // Assert
        var filePath = Path.Combine(LocalizationRoot, $"{TestGuildId}.json");
        Assert.True(_fileSystem.FileExists(filePath));

        var savedLanguage = JsonSerializer.Deserialize<string>(_fileSystem.ReadAllText(filePath));
        Assert.Equal(LanguageCodeEng, savedLanguage);
    }

    [Fact]
    public void LoadLanguage_UsesGuildSpecificFileIfExists()
    {
        // Arrange
        var service = new LocalizationService(_loggerMock.Object, _fileSystem);

        _fileSystem.WriteAllText(
            Path.Combine(LocalizationRoot, $"{TestGuildId}.json"),
            JsonSerializer.Serialize(LanguageCodeEng)
        );

        CreateTranslationFile(LanguageCodeEng, new Dictionary<string, string> { { TestKeyHello, TestValueHelloGuild } });

        // Act
        service.LoadLanguage(TestGuildId);

        var result = service.Get(TestKeyHello);

        // Assert
        Assert.Equal(TestValueHelloGuild, result);
    }

    private void CreateTranslationFile(string languageCode, Dictionary<string, string> translations)
    {
        var filePath = Path.Combine(TranslationRoot, $"{languageCode}.json");

        _fileSystem.WriteAllText(filePath, JsonSerializer.Serialize(translations));
    }
}