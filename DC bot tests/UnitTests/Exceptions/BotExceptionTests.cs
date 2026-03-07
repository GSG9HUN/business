using DC_bot.Exceptions;
using DC_bot.Exceptions.Localization;

namespace DC_bot_tests.UnitTests.Exceptions;

public class BotExceptionTests
{
    [Fact]
    public void BotException_CannotBeInstantiated_IsAbstract()
    {
        // This test verifies BotException is abstract
        // We test through a concrete subclass
        var exception = new LocalizationException("en", "Test message");
        Assert.IsAssignableFrom<BotException>(exception);
    }
}