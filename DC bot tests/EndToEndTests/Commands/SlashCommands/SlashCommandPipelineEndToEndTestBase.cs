using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.SlashCommands;
using Moq;

namespace DC_bot_tests.EndToEndTests.Commands.SlashCommands;

public abstract class SlashCommandPipelineEndToEndTestBase
{
    private readonly SlashCommandTestGraph _graph = new(useSavedGuildLanguage: true);

    protected Mock<ILavaLinkService> LavaLinkServiceMock => _graph.LavaLinkServiceMock;
    protected Mock<ICurrentTrackService> CurrentTrackServiceMock => _graph.CurrentTrackServiceMock;
    protected Mock<IMusicQueueService> MusicQueueServiceMock => _graph.MusicQueueServiceMock;
    protected Mock<IRepeatService> RepeatServiceMock => _graph.RepeatServiceMock;
    protected Mock<ITrackFormatterService> TrackFormatterServiceMock => _graph.TrackFormatterServiceMock;
    protected Mock<ILocalizationService> LocalizationServiceMock => _graph.LocalizationServiceMock;
    protected ISlashCommandExecutor SlashCommandExecutor => _graph.Executor;

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
}
