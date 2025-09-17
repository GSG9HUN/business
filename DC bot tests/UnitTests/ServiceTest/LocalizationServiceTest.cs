using System.Text.Json;
using DC_bot.Service;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.ServiceTest;

public class LocalizationServiceTest
{
    private readonly Mock<ILogger<LocalizationService>> _loggerMock;
    private readonly string _tempLocalizationDirectory;
    private readonly string _tempTranslationDirectory;

    public LocalizationServiceTest()
    {
        _loggerMock = new Mock<ILogger<LocalizationService>>();

        // Ideiglenes könyvtárak létrehozása a tesztekhez
        _tempLocalizationDirectory = Path.Combine(Path.GetTempPath(), "localization");
        _tempTranslationDirectory = Path.Combine(Path.GetTempPath(), "translations");

        LocalizationService.LocalizationDirectory = _tempLocalizationDirectory;
        LocalizationService.TranslationDirectory = _tempTranslationDirectory;

        Directory.CreateDirectory(_tempLocalizationDirectory);
        Directory.CreateDirectory(_tempTranslationDirectory);
    }

    [Fact]
    public void Constructor_CreatesLocalizationDirectory_WhenNotExists()
    {
        // Arrange
        var tempLocalizationDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        LocalizationService.LocalizationDirectory = tempLocalizationDirectory;
        // Act
        var service = new LocalizationService(_loggerMock.Object);

        // Assert
        Assert.True(Directory.Exists(tempLocalizationDirectory));

        // Cleanup
        if (Directory.Exists(tempLocalizationDirectory))
            Directory.Delete(tempLocalizationDirectory, true);
    }

    [Fact]
    public void LoadTranslations_ThrowsFileNotFoundException_WhenLanguageFileMissing()
    {
        // Arrange
        var service = new LocalizationService(_loggerMock.Object);
        DeleteTempFiles();
        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => service.LoadLanguage(12345));
    }

    [Fact]
    public void Get_ReturnsKey_WhenTranslationMissing()
    {
        // Arrange
        var service = new LocalizationService(_loggerMock.Object);
        CreateTranslationFile("eng", new Dictionary<string, string> { { "Hello", "Hello World" } });
        service.LoadLanguage(12345);

        // Act
        var result = service.Get("MissingKey");

        // Assert
        Assert.Equal("MissingKey", result);
    }

    [Fact]
    public void Get_ReturnsTranslatedValue_WhenKeyExists()
    {
        // Arrange
        var service = new LocalizationService(_loggerMock.Object);
        CreateTranslationFile("eng", new Dictionary<string, string> { { "Hello", "Hello World" } });
        service.LoadLanguage(12345);

        // Act
        var result = service.Get("Hello");

        // Assert
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void SaveLanguage_CreatesGuildSpecificFile()
    {
        // Arrange
        var service = new LocalizationService(_loggerMock.Object);

        // Act
        service.SaveLanguage(12345, "eng");

        // Assert
        var filePath = Path.Combine(_tempLocalizationDirectory, "12345.json");
        Assert.True(File.Exists(filePath));

        var savedLanguage = JsonSerializer.Deserialize<string>(File.ReadAllText(filePath));
        Assert.Equal("eng", savedLanguage);
    }

    [Fact]
    public void LoadLanguage_UsesGuildSpecificFileIfExists()
    {
        // Arrange
        var service = new LocalizationService(_loggerMock.Object);

        File.WriteAllText(
            Path.Combine(_tempLocalizationDirectory, "12345.json"),
            JsonSerializer.Serialize("eng")
        );

        CreateTranslationFile("eng", new Dictionary<string, string> { { "Hello", "Hello Guild" } });

        // Act
        service.LoadLanguage(12345);

        var result = service.Get("Hello");

        // Assert
        Assert.Equal("Hello Guild", result);
    }


    private void CreateTranslationFile(string languageCode, Dictionary<string, string> translations)
    {
        var filePath = Path.Combine(_tempTranslationDirectory, $"{languageCode}.json");

        File.WriteAllText(filePath, JsonSerializer.Serialize(translations));
    }

    ~LocalizationServiceTest()
    {
        DeleteTempFiles();
    }

    private void DeleteTempFiles()
    {
        if (Directory.Exists(_tempLocalizationDirectory))
            Directory.Delete(_tempLocalizationDirectory, true);

        if (Directory.Exists(_tempTranslationDirectory))
            Directory.Delete(_tempTranslationDirectory, true);
    }
}