using DC_bot.Interface.Service.Persistence.Models.MobileAppUserSettings;

namespace DC_bot_tests.UnitTests.Model;

[Trait("Category", "Unit")]
public class MobileAppLanguageCodeTests
{
    [Theory]
    [InlineData("en", "en")]
    [InlineData("EN", "en")]
    [InlineData(" hu ", "hu")]
    public void TryNormalize_WhenLanguageCodeIsSupported_ReturnsNormalizedValue(
        string input,
        string expected)
    {
        var result = MobileAppLanguageCode.TryNormalize(input, out var normalized);

        Assert.True(result);
        Assert.Equal(expected, normalized);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("de")]
    [InlineData("english")]
    [InlineData("toolongcode1")]
    public void TryNormalize_WhenLanguageCodeIsInvalid_ReturnsFalse(string? input)
    {
        var result = MobileAppLanguageCode.TryNormalize(input, out var normalized);

        Assert.False(result);
        Assert.Equal(string.Empty, normalized);
    }
}
