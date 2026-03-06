using DC_bot.Exceptions.Localization;

namespace DC_bot_tests.UnitTests.Exceptions.Localization;

public class LocalizationExceptionTests
{
    [Fact]
    public void LocalizationException_MessageIsSet()
    {
        // Arrange
        const string message = "Localization failed";
        const string languageCode = "en";

        // Act
        var exception = new LocalizationException(languageCode, message);

        // Assert
        Assert.Contains(message, exception.Message);
        Assert.Equal(languageCode, exception.LanguageCode);
    }

    [Fact]
    public void LocalizationException_WithInnerException_PreservesIt()
    {
        // Arrange
        const string message = "Localization failed";
        const string languageCode = "hu";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new LocalizationException(languageCode, message, innerException);

        // Assert
        Assert.Equal(innerException, exception.InnerException);
        Assert.Equal(languageCode, exception.LanguageCode);
    }

    [Fact]
    public void LocalizationException_LanguageCodeIsAccessible()
    {
        // Arrange
        const string languageCode = "de";

        // Act
        var exception = new LocalizationException(languageCode, "Test");

        // Assert
        Assert.Equal(languageCode, exception.LanguageCode);
    }

    [Fact]
    public void LocalizationException_MessageIncludesLanguageCode()
    {
        // Arrange
        const string languageCode = "fr";

        // Act
        var exception = new LocalizationException(languageCode, "Test message");

        // Assert
        Assert.Contains(languageCode, exception.Message);
    }
}