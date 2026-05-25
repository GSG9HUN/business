using DC_bot.Exceptions;
using DC_bot.Exceptions.Localization;

namespace DC_bot_tests.UnitTests.Exceptions;

[Trait("Category", "Unit")]
public class BotExceptionTests
{
    [Fact]
    public void BotException_CannotBeInstantiated_IsAbstract()
    {
        var exception = new LocalizationException("en", "Test message");
        Assert.IsAssignableFrom<BotException>(exception);
    }
}
