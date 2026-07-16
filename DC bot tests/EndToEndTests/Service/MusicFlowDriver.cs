using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Service.ReactionHandler;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace DC_bot_tests.EndToEndTests.Service;

internal sealed class MusicFlowDriver
{
    private readonly DiscordE2EClientFixture _discordFixture;
    private readonly ITestOutputHelper _testOutputHelper;

    private MusicFlowDriver(
        DiscordE2EClientFixture discordFixture,
        LiveMusicFlowMessage message,
        IReadOnlyCollection<string> channelMessages,
        IDiscordMember member,
        ITestOutputHelper testOutputHelper)
    {
        _discordFixture = discordFixture;
        Message = message;
        ChannelMessages = channelMessages;
        Member = member;
        _testOutputHelper = testOutputHelper;
    }

    public LiveMusicFlowMessage Message { get; }
    public IReadOnlyCollection<string> ChannelMessages { get; }
    public IDiscordMember Member { get; }
    public bool LeaveExecuted { get; private set; }

    public static MusicFlowDriver Create(
        DiscordE2EClientFixture discordFixture,
        ITestOutputHelper testOutputHelper)
    {
        var commandContext = CreateCommandContext(
            discordFixture.Guild,
            discordFixture.GuildId,
            discordFixture.TextChannel,
            discordFixture.VoiceChannel.Id,
            discordFixture.VoiceChannel);

        return new MusicFlowDriver(
            discordFixture,
            commandContext.Message,
            commandContext.ChannelMessages,
            commandContext.Member,
            testOutputHelper);
    }

    public async Task ExecuteCommandAsync(string commandName, string commandText)
    {
        await _discordFixture.TextChannel.SendMessageAsync($"E2E executing: `{commandText}`");
        Message.Content = commandText;
        await GetCommand(_discordFixture.Provider, commandName).ExecuteAsync(Message);
        WriteDiagnostics($"after {commandText}");
    }

    public async Task LeaveAsync()
    {
        await ExecuteCommandAsync("leave", "!leave");
        LeaveExecuted = true;
    }

    public async Task TryLeaveForCleanupAsync()
    {
        try
        {
            Message.Content = "!leave";
            await GetCommand(_discordFixture.Provider, "leave").ExecuteAsync(Message);
        }
        catch
        {
            // Best-effort cleanup after an E2E playback failure.
        }
    }

    public async Task ExecuteReactionAddedAsync(string emojiName)
    {
        var handler = new ReactionHandlerService(
            _discordFixture.Provider.GetRequiredService<ILavaLinkService>(),
            _discordFixture.Provider.GetRequiredService<ILogger<ReactionHandlerService>>(),
            _discordFixture.Provider.GetRequiredService<IProgressiveTimerService>(),
            _discordFixture.Provider.GetRequiredService<ILocalizationService>(),
            isTestMode: true);

        handler.RegisterHandler(_discordFixture.Client);
        try
        {
            var discordMember = await _discordFixture.Guild.GetMemberAsync(_discordFixture.Client.CurrentUser.Id);
            var message = await _discordFixture.TextChannel.SendMessageAsync($"e2e-reaction-control-{emojiName}");
            var emoji = DiscordEmoji.FromName(_discordFixture.Client, emojiName);
            var args = DiscordEventArgsFactory.CreateMessageReactionAdded(
                message,
                discordMember,
                _discordFixture.TextChannel,
                emoji,
                _discordFixture.Guild);

            await handler.HandleEventAsync(_discordFixture.Client, args);
        }
        finally
        {
            handler.UnregisterHandler(_discordFixture.Client);
        }
    }

    private static ICommand GetCommand(IServiceProvider provider, string name)
    {
        return provider.GetServices<ICommand>().Single(command => command.Name == name);
    }

    private static LiveCommandContext CreateCommandContext(
        DiscordGuild discordGuild,
        ulong guildId,
        DiscordChannel discordTextChannel,
        ulong voiceChannelId,
        DiscordChannel discordVoiceChannel)
    {
        const ulong testUserId = 9_001UL;
        var user = new Mock<IDiscordUser>();
        user.SetupGet(x => x.Id).Returns(testUserId);
        user.SetupGet(x => x.Username).Returns("E2EPlaybackUser");
        user.SetupGet(x => x.Mention).Returns("<@9001>");
        user.SetupGet(x => x.IsBot).Returns(false);

        var guild = new Mock<IDiscordGuild>();
        var channelMessages = new List<string>();
        var textChannel = new Mock<IDiscordChannel>();
        var voiceChannel = new Mock<IDiscordChannel>();
        var voiceState = new Mock<IDiscordVoiceState>();
        var member = new Mock<IDiscordMember>();

        guild.SetupGet(x => x.Id).Returns(guildId);
        guild.SetupGet(x => x.Name).Returns(discordGuild.Name);
        guild.Setup(x => x.ToDiscordGuild()).Returns(discordGuild);
        guild.Setup(x => x.GetMemberAsync(testUserId)).ReturnsAsync(member.Object);
        guild.Setup(x => x.GetAllMembersAsync()).ReturnsAsync([member.Object]);

        textChannel.SetupGet(x => x.Id).Returns(discordTextChannel.Id);
        textChannel.SetupGet(x => x.Name).Returns(discordTextChannel.Name);
        textChannel.SetupGet(x => x.Guild).Returns(guild.Object);
        textChannel.Setup(x => x.SendMessageAsync(It.IsAny<string>()))
            .Callback<string>(channelMessages.Add)
            .Returns<string>(async content => await discordTextChannel.SendMessageAsync(content));
        textChannel.Setup(x => x.SendMessageAsync(It.IsAny<DiscordEmbed>()))
            .Returns<DiscordEmbed>(async embed => await discordTextChannel.SendMessageAsync(embed));
        textChannel.Setup(x => x.ToDiscordChannel()).Returns(discordTextChannel);

        voiceChannel.SetupGet(x => x.Id).Returns(voiceChannelId);
        voiceChannel.SetupGet(x => x.Name).Returns("e2e-music-flow-voice");
        voiceChannel.SetupGet(x => x.Guild).Returns(guild.Object);
        voiceChannel.Setup(x => x.ToDiscordChannel()).Returns(discordVoiceChannel);

        voiceState.SetupGet(x => x.Channel).Returns(voiceChannel.Object);

        member.SetupGet(x => x.Id).Returns(testUserId);
        member.SetupGet(x => x.Username).Returns("E2EPlaybackUser");
        member.SetupGet(x => x.Mention).Returns("<@9001>");
        member.SetupGet(x => x.IsBot).Returns(false);
        member.SetupGet(x => x.VoiceState).Returns(voiceState.Object);

        return new LiveCommandContext(
            new LiveMusicFlowMessage(textChannel.Object, user.Object),
            channelMessages,
            member.Object);
    }

    private void WriteDiagnostics(string phase)
    {
        _testOutputHelper.WriteLine(
            $"{phase}: TextResponses=[{string.Join(" | ", Message.TextResponses)}], ChannelMessages=[{string.Join(" | ", ChannelMessages)}], Embeds={Message.EmbedResponses.Count}");
    }

    private sealed record LiveCommandContext(
        LiveMusicFlowMessage Message,
        IReadOnlyCollection<string> ChannelMessages,
        IDiscordMember Member);
}
