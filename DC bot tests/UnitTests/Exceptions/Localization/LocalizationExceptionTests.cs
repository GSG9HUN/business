using DC_bot.Exceptions.Localization;

namespace DC_bot_tests.UnitTests.Exceptions.Localization;

[Trait("Category", "Unit")]
public class LocalizationExceptionTests
{
    [Fact]
    public void LocalizationException_MessageIsSet()
    {
        const string message = "Localization failed";
        const string languageCode = "en";

        var exception = new LocalizationException(languageCode, message);

        Assert.Contains(message, exception.Message);
        Assert.Equal(languageCode, exception.LanguageCode);
    }

    [Fact]
    public void LocalizationException_WithInnerException_PreservesIt()
    {
        const string message = "Localization failed";
        const string languageCode = "hu";
        var innerException = new InvalidOperationException("Inner error");

        var exception = new LocalizationException(languageCode, message, innerException);

        Assert.Equal(innerException, exception.InnerException);
        Assert.Equal(languageCode, exception.LanguageCode);
    }

    [Fact]
    public void LocalizationException_LanguageCodeIsAccessible()
    {
        const string languageCode = "de";

        var exception = new LocalizationException(languageCode, "Test");

        Assert.Equal(languageCode, exception.LanguageCode);
    }

    [Fact]
    public void LocalizationException_MessageIncludesLanguageCode()
    {
        const string languageCode = "fr";

        var exception = new LocalizationException(languageCode, "Test message");

        Assert.Contains(languageCode, exception.Message);
    }
}
