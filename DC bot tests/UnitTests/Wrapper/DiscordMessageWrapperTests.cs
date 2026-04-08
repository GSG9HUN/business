using DC_bot.Interface.Discord;
using DC_bot.Wrapper;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Wrapper;

public class DiscordMessageWrapperTests
{
    private static DiscordMessageWrapper CreateWrapper(
        ulong id = 1ul,
        string content = "hello",
        IDiscordChannel? channel = null,
        IDiscordUser? author = null,
        Func<string, Task<DiscordMessage>>? respondString = null,
        Func<DiscordEmbed, Task<DiscordMessage>>? respondEmbed = null,
        Func<DiscordMessageBuilder, Task<DiscordMessage>>? modify = null,
        ILogger<DiscordMessageWrapper>? logger = null)
    {
        return new DiscordMessageWrapper(
            id,
            content,
            channel ?? new Mock<IDiscordChannel>().Object,
            author ?? new Mock<IDiscordUser>().Object,
            DateTimeOffset.UtcNow,
            [],
            respondString ?? (_ => Task.FromResult<DiscordMessage>(null!)),
            respondEmbed ?? (_ => Task.FromResult<DiscordMessage>(null!)),
            modify ?? (_ => Task.FromResult<DiscordMessage>(null!)),
            logger);
    }

    [Fact]
    public void Properties_AreSetFromConstructorArguments()
    {
        var channelMock = new Mock<IDiscordChannel>();
        var authorMock = new Mock<IDiscordUser>();
        var createdAt = DateTimeOffset.UtcNow;
        var embeds = new List<DiscordEmbed>();

        var wrapper = new DiscordMessageWrapper(
            42ul, "test content", channelMock.Object, authorMock.Object,
            createdAt, embeds,
            _ => Task.FromResult<DiscordMessage>(null!),
            _ => Task.FromResult<DiscordMessage>(null!),
            _ => Task.FromResult<DiscordMessage>(null!));

        Assert.Equal(42ul, wrapper.Id);
        Assert.Equal("test content", wrapper.Content);
        Assert.Equal(channelMock.Object, wrapper.Channel);
        Assert.Equal(authorMock.Object, wrapper.Author);
        Assert.Equal(createdAt, wrapper.CreatedAt);
        Assert.Empty(wrapper.Embeds);
    }

    [Fact]
    public async Task RespondAsync_String_CallsDelegate()
    {
        string? received = null;
        var wrapper = CreateWrapper(respondString: msg =>
        {
            received = msg;
            return Task.FromResult<DiscordMessage>(null!);
        });

        await wrapper.RespondAsync("hello world");

        Assert.Equal("hello world", received);
    }

    [Fact]
    public async Task RespondAsync_String_WhenDelegateThrows_LogsEventId3001AndDoesNotThrow()
    {
        var loggerMock = new Mock<ILogger<DiscordMessageWrapper>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var ex = new InvalidOperationException("fail");

        var wrapper = CreateWrapper(
            respondString: _ => throw ex,
            logger: loggerMock.Object);

        await wrapper.RespondAsync("oops");

        loggerMock.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Error),
            It.Is<EventId>(e => e.Id == 3001),
            It.IsAny<It.IsAnyType>(),
            It.Is<Exception>(e => e == ex),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task RespondAsync_Embed_CallsDelegate()
    {
        DiscordEmbed? received = null;
        var embed = new DiscordEmbedBuilder().WithTitle("test").Build();

        var wrapper = CreateWrapper(respondEmbed: e =>
        {
            received = e;
            return Task.FromResult<DiscordMessage>(null!);
        });

        await wrapper.RespondAsync(embed);

        Assert.Equal(embed, received);
    }

    [Fact]
    public async Task RespondAsync_Embed_WhenDelegateThrows_LogsEventId3001AndDoesNotThrow()
    {
        var loggerMock = new Mock<ILogger<DiscordMessageWrapper>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var ex = new InvalidOperationException("fail");
        var embed = new DiscordEmbedBuilder().Build();

        var wrapper = CreateWrapper(
            respondEmbed: _ => throw ex,
            logger: loggerMock.Object);

        await wrapper.RespondAsync(embed);

        loggerMock.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Error),
            It.Is<EventId>(e => e.Id == 3001),
            It.IsAny<It.IsAnyType>(),
            It.Is<Exception>(e => e == ex),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ModifyAsync_CallsDelegate()
    {
        DiscordMessageBuilder? received = null;
        var builder = new DiscordMessageBuilder().WithContent("updated");

        var wrapper = CreateWrapper(modify: b =>
        {
            received = b;
            return Task.FromResult<DiscordMessage>(null!);
        });

        await wrapper.ModifyAsync(builder);

        Assert.Equal(builder, received);
    }

    [Fact]
    public async Task ModifyAsync_WhenDelegateThrows_LogsEventId3001AndDoesNotThrow()
    {
        var loggerMock = new Mock<ILogger<DiscordMessageWrapper>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var ex = new InvalidOperationException("fail");

        var wrapper = CreateWrapper(
            modify: _ => throw ex,
            logger: loggerMock.Object);

        await wrapper.ModifyAsync(new DiscordMessageBuilder());

        loggerMock.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Error),
            It.Is<EventId>(e => e.Id == 3001),
            It.IsAny<It.IsAnyType>(),
            It.Is<Exception>(e => e == ex),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

