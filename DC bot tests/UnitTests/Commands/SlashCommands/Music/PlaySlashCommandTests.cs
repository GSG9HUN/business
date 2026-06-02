using DC_bot.Commands.SlashCommands.Music;
using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.SlashCommands;
using Lavalink4NET.Rest.Entities.Tracks;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.SlashCommands.Music;

[Trait("Category", "Unit")]
public class PlaySlashCommandTests : SlashCommandTestBase
{
    [Fact]
    public async Task Play_ShouldCreateInteractionContextAndDelegateQueryToExecutor()
    {
        var dsharpContext = CreateDSharpContext();
        var slashContext = new Mock<ISlashInteractionContext>();
        var executor = CreateModuleExecutor();
        var contextFactory = CreateContextFactory(dsharpContext, slashContext.Object);
        var command = new PlaySlashCommand(executor.Object, contextFactory.Object);

        await command.Play(dsharpContext, Query);

        contextFactory.Verify(x => x.Create(dsharpContext), Times.Once);
        VerifyRequest(
            executor,
            "play",
            slashContext.Object,
            Query,
            requireGuild: true,
            defer: true,
            ensureDeferredResponse: true);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserIsNotInVoiceChannel_ShouldReturnValidationError()
    {
        var member = CreateMember("SlashUser", "<@123>", voiceChannel: null);
        var context = CreateContext(member: member);

        await ExecuteSlashAsync(
            SlashCommandExecutor,
            "play",
            context,
            Query,
            requireGuild: true,
            defer: true,
            ensureDeferredResponse: true);

        Assert.True(context.IsDeferred);
        Assert.Contains("You must be in a voice channel.", context.TextResponses);
        VerifyNoPlaybackStarted();
    }

    [Fact]
    public async Task ExecuteAsync_WithUrl_ShouldCallPlayAsyncUrl()
    {
        var voiceChannel = CreateVoiceChannel();
        var member = CreateMember("SlashUser", "<@123>", voiceChannel);
        var context = CreateContext(member: member);

        await ExecuteSlashAsync(
            SlashCommandExecutor,
            "play",
            context,
            "https://www.youtube.com/watch?v=zIRszCXKzGc",
            requireGuild: true,
            defer: true,
            ensureDeferredResponse: true);

        LavaLinkServiceMock.Verify(
            service => service.PlayAsyncUrl(
                voiceChannel,
                It.Is<Uri>(uri => uri.Host.Contains("youtube", StringComparison.OrdinalIgnoreCase)),
                It.IsAny<IDiscordMessage>(),
                TrackSearchMode.YouTube),
            Times.Once);
        Assert.Contains("Request accepted.", context.TextResponses);
    }

    [Fact]
    public async Task ExecuteAsync_WithQuery_ShouldCallPlayAsyncQuery()
    {
        var voiceChannel = CreateVoiceChannel();
        var member = CreateMember("SlashUser", "<@123>", voiceChannel);
        var context = CreateContext(member: member);

        await ExecuteSlashAsync(
            SlashCommandExecutor,
            "play",
            context,
            Query,
            requireGuild: true,
            defer: true,
            ensureDeferredResponse: true);

        LavaLinkServiceMock.Verify(
            service => service.PlayAsyncQuery(
                voiceChannel,
                Query,
                It.IsAny<IDiscordMessage>(),
                TrackSearchMode.YouTube),
            Times.Once);
        Assert.Contains("Request accepted.", context.TextResponses);
    }

    [Fact]
    public async Task ExecuteAsync_OutsideGuild_ShouldNotDeferAndReturnGuildOnlyMessage()
    {
        var context = new TestSlashInteractionContext();

        await ExecuteSlashAsync(
            SlashCommandExecutor,
            "play",
            context,
            Query,
            requireGuild: true,
            defer: true,
            ensureDeferredResponse: true);

        Assert.False(context.IsDeferred);
        Assert.Contains("This command can only be used in a server.", context.TextResponses);
        VerifyNoPlaybackStarted();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCommandIsMissing_ShouldReturnLocalizedNotRegisteredMessage()
    {
        var context = CreateContext();
        var executor = CreateExecutorWithCommands(Array.Empty<ICommand>());

        await ExecuteSlashAsync(
            executor,
            "play",
            context,
            Query,
            requireGuild: true,
            defer: true,
            ensureDeferredResponse: true);

        Assert.False(context.IsDeferred);
        Assert.Contains("Command 'play' is not registered.", context.TextResponses);
        VerifyNoPlaybackStarted();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCommandThrowsUnexpectedException_ShouldReturnLocalizedError()
    {
        var commandMock = new Mock<ICommand>();
        commandMock.SetupGet(command => command.Name).Returns("play");
        commandMock.SetupGet(command => command.Description).Returns("Plays a song");
        commandMock
            .Setup(command => command.ExecuteAsync(It.IsAny<IDiscordMessage>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected failure"));

        var context = CreateContext();
        var executor = CreateExecutorWithCommands(commandMock.Object);

        await ExecuteSlashAsync(
            executor,
            "play",
            context,
            Query,
            requireGuild: true,
            defer: true,
            ensureDeferredResponse: true);

        Assert.True(context.IsDeferred);
        Assert.Contains("An unexpected error occurred while executing the command.", context.TextResponses);
        VerifyNoPlaybackStarted();
    }
}
