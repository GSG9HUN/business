using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Lavalink4NET.Rest.Entities.Tracks;
using Moq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DC_bot_tests.UnitTests.Commands.SlashCommands;

public abstract class SlashCommandTestBase
{
    protected const string Query = "madeon imperium";
    protected const string MemberMention = "<@999>";
    protected const string DiscordMemberMention = "<@!999>";

    private readonly SlashCommandTestGraph _graph = new();

    protected Mock<ILavaLinkService> LavaLinkServiceMock => _graph.LavaLinkServiceMock;
    protected Mock<ICurrentTrackService> CurrentTrackServiceMock => _graph.CurrentTrackServiceMock;
    protected Mock<IMusicQueueService> MusicQueueServiceMock => _graph.MusicQueueServiceMock;
    protected Mock<IRepeatService> RepeatServiceMock => _graph.RepeatServiceMock;
    protected Mock<ITrackFormatterService> TrackFormatterServiceMock => _graph.TrackFormatterServiceMock;
    protected Mock<ILocalizationService> LocalizationServiceMock => _graph.LocalizationServiceMock;
    protected ILocalizationService LocalizationService => _graph.LocalizationService;
    protected ISlashCommandExecutor SlashCommandExecutor => _graph.Executor;

    protected ISlashCommandExecutor CreateExecutorWithCommands(params ICommand[] commands)
    {
        return _graph.CreateExecutorWithCommands(commands);
    }

    protected static Task ExecuteSlashAsync(
        ISlashCommandExecutor executor,
        string commandName,
        ISlashInteractionContext context,
        string? argument = null,
        bool requireGuild = false,
        bool defer = false,
        bool ensureDeferredResponse = false)
    {
        return executor.ExecuteAsync(new SlashCommandExecutionRequest(
            commandName,
            context,
            argument,
            requireGuild,
            defer,
            ensureDeferredResponse));
    }

    protected static SlashCommandContext CreateDSharpContext()
    {
        // SlashCommandContext is framework-owned; these tests mock the factory boundary.
        return null!;
    }

    protected static DiscordMember CreateDiscordMember(ulong id)
    {
        var member = (DiscordMember)RuntimeHelpers.GetUninitializedObject(typeof(DiscordMember));
        var idField = GetAllFields(typeof(DiscordMember))
            .FirstOrDefault(field => field.Name == "<Id>k__BackingField");

        Assert.NotNull(idField);
        idField.SetValue(member, id);
        return member;
    }

    protected static Mock<ISlashCommandExecutor> CreateModuleExecutor()
    {
        var executor = new Mock<ISlashCommandExecutor>();
        executor
            .Setup(x => x.ExecuteAsync(It.IsAny<SlashCommandExecutionRequest>()))
            .Returns(Task.CompletedTask);
        return executor;
    }

    protected static Mock<ISlashInteractionContextFactory> CreateContextFactory(
        SlashCommandContext dsharpContext,
        ISlashInteractionContext slashContext)
    {
        var contextFactory = new Mock<ISlashInteractionContextFactory>();
        contextFactory.Setup(x => x.Create(dsharpContext)).Returns(slashContext);
        return contextFactory;
    }

    protected static TestSlashInteractionContext CreateContext(
        IDiscordMember? member = null,
        IReadOnlyCollection<IDiscordMember>? allMembers = null)
    {
        var guild = new Mock<IDiscordGuild>();
        guild.SetupGet(x => x.Id).Returns(123UL);
        guild.SetupGet(x => x.Name).Returns("SlashGuild");

        var effectiveMember = member ?? CreateMember("SlashUser", "<@123>", CreateVoiceChannel());
        guild.Setup(x => x.GetMemberAsync(It.IsAny<ulong>())).ReturnsAsync(effectiveMember);
        guild.Setup(x => x.GetAllMembersAsync()).ReturnsAsync(allMembers ?? [effectiveMember]);

        var channel = new Mock<IDiscordChannel>();
        channel.SetupGet(x => x.Id).Returns(456UL);
        channel.SetupGet(x => x.Name).Returns("slash-test");
        channel.SetupGet(x => x.Guild).Returns(guild.Object);

        var user = new Mock<IDiscordUser>();
        user.SetupGet(x => x.Id).Returns(123UL);
        user.SetupGet(x => x.Username).Returns("SlashUser");
        user.SetupGet(x => x.Mention).Returns("<@123>");
        user.SetupGet(x => x.IsBot).Returns(false);

        return new TestSlashInteractionContext(channel.Object, user.Object, guild.Object, effectiveMember);
    }

    protected static IDiscordMember CreateMember(
        string username,
        string mention,
        IDiscordChannel? voiceChannel = null)
    {
        var voiceState = new Mock<IDiscordVoiceState>();
        voiceState.SetupGet(x => x.Channel).Returns(voiceChannel);

        var member = new Mock<IDiscordMember>();
        member.SetupGet(x => x.Id).Returns(123UL);
        member.SetupGet(x => x.Username).Returns(username);
        member.SetupGet(x => x.Mention).Returns(mention);
        member.SetupGet(x => x.IsBot).Returns(false);
        member.SetupGet(x => x.VoiceState).Returns(voiceChannel is null ? null : voiceState.Object);
        return member.Object;
    }

    protected static IDiscordChannel CreateVoiceChannel()
    {
        var guild = new Mock<IDiscordGuild>();
        guild.SetupGet(x => x.Id).Returns(123UL);
        guild.SetupGet(x => x.Name).Returns("SlashGuild");

        var channel = new Mock<IDiscordChannel>();
        channel.SetupGet(x => x.Id).Returns(789UL);
        channel.SetupGet(x => x.Name).Returns("voice");
        channel.SetupGet(x => x.Guild).Returns(guild.Object);
        return channel.Object;
    }

    protected static ILavaLinkTrack CreateTrack(string title, string author)
    {
        var track = new Mock<ILavaLinkTrack>();
        track.SetupGet(x => x.Title).Returns(title);
        track.SetupGet(x => x.Author).Returns(author);
        track.SetupGet(x => x.Duration).Returns(TimeSpan.FromMinutes(3));
        return track.Object;
    }

    protected static void VerifyRequest(
        Mock<ISlashCommandExecutor> executor,
        string commandName,
        ISlashInteractionContext context,
        string? argument = null,
        bool requireGuild = false,
        bool defer = false,
        bool ensureDeferredResponse = false)
    {
        executor.Verify(
            x => x.ExecuteAsync(It.Is<SlashCommandExecutionRequest>(request =>
                request.CommandName == commandName &&
                request.Context == context &&
                request.Argument == argument &&
                request.RequireGuild == requireGuild &&
                request.Defer == defer &&
                request.EnsureDeferredResponse == ensureDeferredResponse)),
            Times.Once);
    }

    protected void VerifyNoPlaybackStarted()
    {
        LavaLinkServiceMock.Verify(
            service => service.PlayAsyncUrl(
                It.IsAny<IDiscordChannel>(),
                It.IsAny<Uri>(),
                It.IsAny<IDiscordMessage>(),
                It.IsAny<TrackSearchMode>()),
            Times.Never);
        LavaLinkServiceMock.Verify(
            service => service.PlayAsyncQuery(
                It.IsAny<IDiscordChannel>(),
                It.IsAny<string>(),
                It.IsAny<IDiscordMessage>(),
                It.IsAny<TrackSearchMode>()),
            Times.Never);
    }

    protected void VerifyMusicControlExecuted(string commandName, IDiscordMember member)
    {
        switch (commandName)
        {
            case "skip":
                LavaLinkServiceMock.Verify(
                    service => service.SkipAsync(It.IsAny<IDiscordMessage>(), member),
                    Times.Once);
                break;
            case "pause":
                LavaLinkServiceMock.Verify(
                    service => service.PauseAsync(It.IsAny<IDiscordMessage>(), member),
                    Times.Once);
                break;
            case "resume":
                LavaLinkServiceMock.Verify(
                    service => service.ResumeAsync(It.IsAny<IDiscordMessage>(), member),
                    Times.Once);
                break;
            case "leave":
                LavaLinkServiceMock.Verify(
                    service => service.LeaveVoiceChannel(It.IsAny<IDiscordMessage>(), member),
                    Times.Once);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(commandName), commandName, null);
        }
    }

    private static IEnumerable<FieldInfo> GetAllFields(Type type)
    {
        for (var currentType = type; currentType is not null; currentType = currentType.BaseType)
        {
            foreach (var field in currentType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                yield return field;
            }
        }
    }
}
