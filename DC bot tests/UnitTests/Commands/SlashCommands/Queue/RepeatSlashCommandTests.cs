using DC_bot.Commands.SlashCommands.Queue;
using DC_bot.Interface;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.SlashCommands.Queue;

[Trait("Category", "Unit")]
public class RepeatSlashCommandTests : SlashCommandTestBase
{
    [Theory]
    [InlineData("track", "repeat")]
    [InlineData("list", "repeatList")]
    public async Task RepeatSubcommand_ShouldCreateInteractionContextAndDelegateToExecutor(
        string subcommandName,
        string textCommandName)
    {
        var dsharpContext = CreateDSharpContext();
        var slashContext = new Mock<ISlashInteractionContext>();
        var executor = CreateModuleExecutor();
        var contextFactory = CreateContextFactory(dsharpContext, slashContext.Object);
        var command = new RepeatSlashCommand(executor.Object, contextFactory.Object);

        await ExecuteRepeatCommand(subcommandName, command, dsharpContext);

        contextFactory.Verify(x => x.Create(dsharpContext), Times.Once);
        VerifyRequest(
            executor,
            textCommandName,
            slashContext.Object,
            requireGuild: true,
            defer: true,
            ensureDeferredResponse: true);
    }

    [Fact]
    public async Task ExecuteAsync_Track_ShouldEnableTrackRepeat()
    {
        RepeatServiceMock.Setup(service => service.IsRepeatingListAsync(123UL)).ReturnsAsync(false);
        RepeatServiceMock.Setup(service => service.IsRepeatingAsync(123UL)).ReturnsAsync(false);
        CurrentTrackServiceMock
            .Setup(service => service.GetCurrentTrackFormattedAsync(123UL, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Track A");
        var context = CreateContext();

        await ExecuteSlashAsync(
            SlashCommandExecutor,
            "repeat",
            context,
            requireGuild: true,
            defer: true,
            ensureDeferredResponse: true);

        Assert.True(context.IsDeferred);
        Assert.Contains("Repeat is on for : Track A", context.TextResponses);
        RepeatServiceMock.Verify(service => service.SetRepeatingAsync(123UL, true), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Track_WhenListIsRepeating_ShouldReturnConflict()
    {
        RepeatServiceMock.Setup(service => service.IsRepeatingListAsync(123UL)).ReturnsAsync(true);
        var context = CreateContext();

        await ExecuteSlashAsync(
            SlashCommandExecutor,
            "repeat",
            context,
            requireGuild: true,
            defer: true,
            ensureDeferredResponse: true);

        Assert.True(context.IsDeferred);
        Assert.Contains("The list is already repeating.", context.TextResponses);
        RepeatServiceMock.Verify(
            service => service.SetRepeatingAsync(It.IsAny<ulong>(), It.IsAny<bool>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_List_ShouldEnableListRepeat()
    {
        var currentTrack = CreateTrack("Track A", "Artist A");
        var queuedTrack = CreateTrack("Track B", "Artist B");
        RepeatServiceMock.Setup(service => service.IsRepeatingAsync(123UL)).ReturnsAsync(false);
        RepeatServiceMock.Setup(service => service.IsRepeatingListAsync(123UL)).ReturnsAsync(false);
        CurrentTrackServiceMock
            .Setup(service => service.GetCurrentTrackAsync(123UL, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentTrack);
        MusicQueueServiceMock
            .Setup(service => service.ViewQueue(123UL))
            .ReturnsAsync([queuedTrack]);
        TrackFormatterServiceMock
            .Setup(service => service.FormatCurrentTrackListAsync(123UL))
            .ReturnsAsync("Track A\nTrack B");
        var context = CreateContext();

        await ExecuteSlashAsync(
            SlashCommandExecutor,
            "repeatList",
            context,
            requireGuild: true,
            defer: true,
            ensureDeferredResponse: true);

        Assert.True(context.IsDeferred);
        Assert.Contains(context.TextResponses, response => response.Contains("Repeat is on for current list:"));
        RepeatServiceMock.Verify(
            service => service.SaveRepeatListSnapshotAsync(
                123UL,
                currentTrack,
                It.Is<IReadOnlyCollection<ILavaLinkTrack>>(tracks => tracks.Contains(queuedTrack))),
            Times.Once);
        RepeatServiceMock.Verify(service => service.SetRepeatingListAsync(123UL, true), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_List_WhenTrackIsRepeating_ShouldReturnConflict()
    {
        RepeatServiceMock.Setup(service => service.IsRepeatingAsync(123UL)).ReturnsAsync(true);
        var context = CreateContext();

        await ExecuteSlashAsync(
            SlashCommandExecutor,
            "repeatList",
            context,
            requireGuild: true,
            defer: true,
            ensureDeferredResponse: true);

        Assert.True(context.IsDeferred);
        Assert.Contains("This track is already repeating.", context.TextResponses);
        RepeatServiceMock.Verify(
            service => service.SetRepeatingListAsync(It.IsAny<ulong>(), It.IsAny<bool>()),
            Times.Never);
    }

    private static Task ExecuteRepeatCommand(
        string subcommandName,
        RepeatSlashCommand command,
        SlashCommandContext dsharpContext)
    {
        return subcommandName switch
        {
            "track" => command.Track(dsharpContext),
            "list" => command.List(dsharpContext),
            _ => throw new ArgumentOutOfRangeException(nameof(subcommandName), subcommandName, null)
        };
    }
}
