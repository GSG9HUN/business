using DC_bot.Wrapper;

namespace DC_bot_tests.UnitTests.Wrapper;

public class DiscordVoiceStateWrapperTests
{

    [Fact]
    public void Channel_WhenVoiceStateIsNull_ReturnsNull()
    {
        var wrapper = new DiscordVoiceStateWrapper(null);
        Assert.Null(wrapper.Channel);
    }

    [Fact]
    public void Channel_WhenVoiceStateHasNoChannel_ReturnsNull()
    {
        var state = DiscordEntityFactory.CreateVoiceState(channelId: null);
        var wrapper = new DiscordVoiceStateWrapper(state);
        Assert.Null(wrapper.Channel);
    }

    [Fact]
    public void ToDiscordVoiceState_WhenVoiceStateIsNull_ThrowsInvalidOperationException()
    {
        var wrapper = new DiscordVoiceStateWrapper(null);
        Assert.Throws<InvalidOperationException>(() => wrapper.ToDiscordVoiceState());
    }

    [Fact]
    public void ToDiscordVoiceState_WhenVoiceStateIsNotNull_ReturnsSameInstance()
    {
        var state = DiscordEntityFactory.CreateVoiceState(channelId: null);
        var wrapper = new DiscordVoiceStateWrapper(state);
        Assert.Equal(state, wrapper.ToDiscordVoiceState());
    }
}
