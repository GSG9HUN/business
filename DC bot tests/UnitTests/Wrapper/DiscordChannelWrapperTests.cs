using DC_bot.Wrapper;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Wrapper;

public class DiscordChannelWrapperTests
{
    [Fact]
    public void Id_ReturnsUnderlyingChannelId()
    {
        var wrapper = new DiscordChannelWrapper(DiscordEntityFactory.CreateChannel(id: 99ul));
        Assert.Equal(99ul, wrapper.Id);
    }

    [Fact]
    public void Name_ReturnsUnderlyingChannelName()
    {
        var wrapper = new DiscordChannelWrapper(DiscordEntityFactory.CreateChannel(name: "general"));
        Assert.Equal("general", wrapper.Name);
    }

    [Fact]
    public void ToDiscordChannel_ReturnsSameInstance()
    {
        var channel = DiscordEntityFactory.CreateChannel();
        var wrapper = new DiscordChannelWrapper(channel);
        Assert.Equal(channel, wrapper.ToDiscordChannel());
    }
    
    [Fact]
    public async Task SendMessageAsync_String_WhenDiscordThrows_LogsEventId3002AndDoesNotThrow()
    {
        var loggerMock = new Mock<ILogger<DiscordChannelWrapper>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        
        var wrapper = new DiscordChannelWrapper(DiscordEntityFactory.CreateChannel(), loggerMock.Object);

        await wrapper.SendMessageAsync("hello");

        loggerMock.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Error),
            It.Is<EventId>(e => e.Id == 3002),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
    
    [Fact]
    public async Task SendMessageAsync_Embed_WhenDiscordThrows_LogsEventId3002AndDoesNotThrow()
    {
        var loggerMock = new Mock<ILogger<DiscordChannelWrapper>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var embed = new DiscordEmbedBuilder().WithTitle("test").Build();

        var wrapper = new DiscordChannelWrapper(DiscordEntityFactory.CreateChannel(), loggerMock.Object);

        await wrapper.SendMessageAsync(embed);

        loggerMock.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Error),
            It.Is<EventId>(e => e.Id == 3002),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
