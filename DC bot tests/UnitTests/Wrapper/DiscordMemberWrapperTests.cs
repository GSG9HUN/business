using DC_bot.Wrapper;

namespace DC_bot_tests.UnitTests.Wrapper;

public class DiscordMemberWrapperTests
{
    [Fact]
    public void Id_ReturnsUnderlyingMemberId()
    {
        var wrapper = new DiscordMemberWrapper(DiscordEntityFactory.CreateMember(id: 42ul));
        Assert.Equal(42ul, wrapper.Id);
    }

    [Fact]
    public void ToDiscordMember_ReturnsSameInstance()
    {
        var member = DiscordEntityFactory.CreateMember();
        var wrapper = new DiscordMemberWrapper(member);
        Assert.Equal(member, wrapper.ToDiscordMember());
    }

    [Fact]
    public void Mention_ReturnsUnderlyingMention()
    {
        var member = DiscordEntityFactory.CreateMember(id: 99ul);
        var wrapper = new DiscordMemberWrapper(member);
        Assert.NotNull(wrapper.Mention);
    }
}
