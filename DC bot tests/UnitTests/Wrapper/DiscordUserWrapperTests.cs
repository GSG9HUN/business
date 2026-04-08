using DC_bot.Wrapper;
using DSharpPlus.Entities;
using Moq;

namespace DC_bot_tests.UnitTests.Wrapper;

public class DiscordUserWrapperTests
{
    [Fact]
    public void Id_ReturnsUnderlyingUserId()
    {
        var wrapper = new DiscordUserWrapper(DiscordEntityFactory.CreateUser(id: 12345ul));
        Assert.Equal(12345ul, wrapper.Id);
    }

    [Fact]
    public void Username_ReturnsUnderlyingUsername()
    {
        var wrapper = new DiscordUserWrapper(DiscordEntityFactory.CreateUser(username: "Alice"));
        Assert.Equal("Alice", wrapper.Username);
    }

    [Fact]
    public void IsBot_WhenTrue_ReturnsTrue()
    {
        var wrapper = new DiscordUserWrapper(DiscordEntityFactory.CreateUser(isBot: true));
        Assert.True(wrapper.IsBot);
    }

    [Fact]
    public void IsBot_WhenFalse_ReturnsFalse()
    {
        var wrapper = new DiscordUserWrapper(DiscordEntityFactory.CreateUser(isBot: false));
        Assert.False(wrapper.IsBot);
    }

    [Fact]
    public void Mention_ReturnsUnderlyingMention()
    {
        var user = DiscordEntityFactory.CreateUser(id: 42ul);
        var wrapper = new DiscordUserWrapper(user);
        
        Assert.NotNull(wrapper.Mention);
    }

    [Fact]
    public void ToDiscordUser_ReturnsSameInstance()
    {
        var user = DiscordEntityFactory.CreateUser();
        var wrapper = new DiscordUserWrapper(user);
        Assert.Equal(user, wrapper.ToDiscordUser());
    }
}
