using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.SlashCommands;
using DC_bot.Wrapper;
using DSharpPlus.Entities;
using Moq;

namespace DC_bot_tests.UnitTests.Wrapper;

[Trait("Category", "Unit")]
public class SlashInteractionMessageWrapperTests
{
    private readonly Mock<IDiscordChannel> _channelMock = new();
    private readonly Mock<IDiscordUser> _userMock = new();
    private readonly Mock<ISlashInteractionContext> _contextMock = new();

    public SlashInteractionMessageWrapperTests()
    {
        _contextMock.SetupGet(context => context.Channel).Returns(_channelMock.Object);
        _contextMock.SetupGet(context => context.User).Returns(_userMock.Object);
        _contextMock.Setup(context => context.RespondAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        _contextMock.Setup(context => context.RespondAsync(It.IsAny<DiscordEmbed>())).Returns(Task.CompletedTask);
    }

    [Fact]
    public void Constructor_MapsContentChannelAndAuthor()
    {
        var wrapper = new SlashInteractionMessageWrapper("ping", _contextMock.Object);

        Assert.Equal("ping", wrapper.Content);
        Assert.Equal(_channelMock.Object, wrapper.Channel);
        Assert.Equal(_userMock.Object, wrapper.Author);
        Assert.Empty(wrapper.Embeds);
        Assert.True(wrapper.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task RespondAsync_WithString_DelegatesToSlashContext()
    {
        var wrapper = new SlashInteractionMessageWrapper("ping", _contextMock.Object);

        await wrapper.RespondAsync("Pong!");

        _contextMock.Verify(context => context.RespondAsync("Pong!"), Times.Once);
    }

    [Fact]
    public async Task RespondAsync_WithEmbed_DelegatesToSlashContext()
    {
        var embed = new DiscordEmbedBuilder().WithTitle("Help").Build();
        var wrapper = new SlashInteractionMessageWrapper("help", _contextMock.Object);

        await wrapper.RespondAsync(embed);

        _contextMock.Verify(context => context.RespondAsync(embed), Times.Once);
    }

    [Fact]
    public async Task ModifyAsync_WithContent_RespondsWithContent()
    {
        var wrapper = new SlashInteractionMessageWrapper("queue", _contextMock.Object);

        await wrapper.ModifyAsync(new DiscordMessageBuilder().WithContent("updated"));

        _contextMock.Verify(context => context.RespondAsync("updated"), Times.Once);
        _contextMock.Verify(context => context.RespondAsync(It.IsAny<DiscordEmbed>()), Times.Never);
    }

    [Fact]
    public async Task ModifyAsync_WithEmbedAndNoContent_RespondsWithFirstEmbed()
    {
        var firstEmbed = new DiscordEmbedBuilder().WithTitle("First").Build();
        var secondEmbed = new DiscordEmbedBuilder().WithTitle("Second").Build();
        var wrapper = new SlashInteractionMessageWrapper("queue", _contextMock.Object);

        await wrapper.ModifyAsync(new DiscordMessageBuilder()
            .AddEmbed(firstEmbed)
            .AddEmbed(secondEmbed));

        _contextMock.Verify(context => context.RespondAsync(firstEmbed), Times.Once);
        _contextMock.Verify(context => context.RespondAsync(secondEmbed), Times.Never);
        _contextMock.Verify(context => context.RespondAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ModifyAsync_WithEmptyBuilder_DoesNotRespond()
    {
        var wrapper = new SlashInteractionMessageWrapper("queue", _contextMock.Object);

        await wrapper.ModifyAsync(new DiscordMessageBuilder());

        _contextMock.Verify(context => context.RespondAsync(It.IsAny<string>()), Times.Never);
        _contextMock.Verify(context => context.RespondAsync(It.IsAny<DiscordEmbed>()), Times.Never);
    }
}
