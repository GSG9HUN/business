using DC_bot.Service.ReactionHandler;
using DC_bot_tests.UnitTests.Wrapper;

namespace DC_bot_tests.UnitTests.Service.ReactionHandler;

[Trait("Category", "Unit")]
public class ReactionContextFactoryTests
{
    [Fact]
    public async Task CreateAsync_WhenGuildIsProvided_ReturnsWrappedContext()
    {
        var guild = DiscordEntityFactory.CreateGuild(id: 123ul);
        var channel = DiscordEntityFactory.CreateChannel(id: 456ul, guild: guild);
        var member = DiscordEntityFactory.CreateMember(id: 789ul, username: "ReactionUser");
        var message = DiscordEntityFactory.CreateMessage(
            id: 321ul,
            content: "controls",
            channel: channel,
            author: member);

        var context = await new ReactionContextFactory().CreateAsync(message, member, channel, guild);

        Assert.Equal(123ul, context.GuildId);
        Assert.Equal(789ul, context.Member.Id);
        Assert.Equal(321ul, context.Message.Id);
    }

    [Fact]
    public async Task CreateAsync_WhenGuildCannotBeResolved_ThrowsInvalidOperationException()
    {
        var channel = DiscordEntityFactory.CreateChannel();
        var user = DiscordEntityFactory.CreateUser();
        var message = DiscordEntityFactory.CreateMessage(channel: channel, author: user);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => new ReactionContextFactory().CreateAsync(message, user, channel));
    }
}
