using DC_bot.Wrapper;

namespace DC_bot_tests.UnitTests.Wrapper;

public class DiscordGuildWrapperTests
{
    [Fact]
    public void Id_ReturnsUnderlyingGuildId()
    {
        var wrapper = new DiscordGuildWrapper(DiscordEntityFactory.CreateGuild(id: 777ul));
        Assert.Equal(777ul, wrapper.Id);
    }

    [Fact]
    public void Name_ReturnsUnderlyingGuildName()
    {
        var wrapper = new DiscordGuildWrapper(DiscordEntityFactory.CreateGuild(name: "MyServer"));
        Assert.Equal("MyServer", wrapper.Name);
    }

    [Fact]
    public void ToDiscordGuild_ReturnsSameInstance()
    {
        var guild = DiscordEntityFactory.CreateGuild();
        var wrapper = new DiscordGuildWrapper(guild);
        Assert.Equal(guild, wrapper.ToDiscordGuild());
    }
}
