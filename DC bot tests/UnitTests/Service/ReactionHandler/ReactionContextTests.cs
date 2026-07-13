using DC_bot.Interface.Discord;
using DC_bot.Service.ReactionHandler;
using Moq;

namespace DC_bot_tests.UnitTests.Service.ReactionHandler;

[Trait("Category", "Unit")]
public class ReactionContextTests
{
    [Fact]
    public void Constructor_AssignsProperties()
    {
        var member = Mock.Of<IDiscordMember>();
        var message = Mock.Of<IDiscordMessage>();

        var context = new ReactionContext(member, message, 123ul);

        Assert.Same(member, context.Member);
        Assert.Same(message, context.Message);
        Assert.Equal(123ul, context.GuildId);
    }
}
