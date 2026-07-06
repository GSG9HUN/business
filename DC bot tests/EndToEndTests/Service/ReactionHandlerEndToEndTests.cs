using DC_bot.Constants;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Service.ReactionHandler;
using DC_bot.Wrapper;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.EndToEndTests.Service;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class ReactionHandlerEndToEndTests : IAsyncLifetime
{
    private readonly ulong _testChannelId;
    private readonly string _controlMarker = $"MusicControl-{Guid.NewGuid():N}";
    private readonly DiscordClient? _discordClient;
    private readonly bool _isConfigured;
    private bool _isDiscordAvailable;
    private readonly Mock<ILavaLinkService> _lavaLinkServiceMock = new();
    private readonly Mock<ILocalizationService> _localizationServiceMock = new();
    private readonly Mock<ILogger<ReactionHandlerService>> _loggerMock = new();
    private readonly Mock<IProgressiveTimerService> _progressiveTimerServiceMock = new();
    private readonly ReactionHandlerService _reactionHandlerService;
    private readonly ReactionHandlerService _productionReactionHandlerService;

    public ReactionHandlerEndToEndTests()
    {
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _localizationServiceMock.Setup(x => x.Get(LocalizationKeys.MusicControl)).Returns(_controlMarker);
        _localizationServiceMock.Setup(x => x.Get(It.IsAny<ulong>(), LocalizationKeys.MusicControl))
            .Returns(_controlMarker);
        _localizationServiceMock.Setup(x => x.Get(LocalizationKeys.PauseReaction)).Returns("Pause");
        _localizationServiceMock.Setup(x => x.Get(It.IsAny<ulong>(), LocalizationKeys.PauseReaction)).Returns("Pause");
        _localizationServiceMock.Setup(x => x.Get(LocalizationKeys.ResumeReaction)).Returns("Resume");
        _localizationServiceMock.Setup(x => x.Get(It.IsAny<ulong>(), LocalizationKeys.ResumeReaction))
            .Returns("Resume");
        _localizationServiceMock.Setup(x => x.Get(LocalizationKeys.SkipReaction)).Returns("Skip");
        _localizationServiceMock.Setup(x => x.Get(It.IsAny<ulong>(), LocalizationKeys.SkipReaction)).Returns("Skip");
        _localizationServiceMock.Setup(x => x.Get(LocalizationKeys.RepeatReaction)).Returns("Repeat");
        _localizationServiceMock.Setup(x => x.Get(It.IsAny<ulong>(), LocalizationKeys.RepeatReaction))
            .Returns("Repeat");
        _localizationServiceMock.Setup(x => x.Get(LocalizationKeys.ReactionHandlerRepeatOn)).Returns("Repeat on");
        _localizationServiceMock.Setup(x => x.Get(It.IsAny<ulong>(), LocalizationKeys.ReactionHandlerRepeatOn))
            .Returns("Repeat on");
        _localizationServiceMock.Setup(x => x.Get(LocalizationKeys.ReactionHandlerRepeatOff)).Returns("Repeat off");
        _localizationServiceMock.Setup(x => x.Get(It.IsAny<ulong>(), LocalizationKeys.ReactionHandlerRepeatOff))
            .Returns("Repeat off");

        _reactionHandlerService = new ReactionHandlerService(
            _lavaLinkServiceMock.Object,
            _loggerMock.Object,
            _progressiveTimerServiceMock.Object,
            _localizationServiceMock.Object, true);

        _productionReactionHandlerService = new ReactionHandlerService(
            _lavaLinkServiceMock.Object,
            _loggerMock.Object,
            _progressiveTimerServiceMock.Object,
            _localizationServiceMock.Object);

        var hasToken = EndToEndTestConfiguration.TryGetDiscordToken(out var token);
        var hasChannel = EndToEndTestConfiguration.TryGetDiscordChannelId(out var testChannelId);
        _testChannelId = testChannelId;

        if (hasToken && hasChannel)
        {
            _isConfigured = true;
            _discordClient = TestDiscordClientFactory.Create(
                token,
                DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers | DiscordIntents.MessageContents |
                DiscordIntents.GuildMessageReactions);
        }
        else
        {
            _isConfigured = false;
        }
    }

    public async Task InitializeAsync()
    {
        if (!_isConfigured || _discordClient == null) return;
        _isDiscordAvailable = await EndToEndDiscordGuard.TryConnectAndWaitUntilReadyAsync(_discordClient);
    }

    public async Task DisposeAsync()
    {
        if (_discordClient != null)
        {
            _reactionHandlerService.UnregisterHandler(_discordClient);
            _productionReactionHandlerService.UnregisterHandler(_discordClient);
            await EndToEndDiscordGuard.DisconnectIgnoringDisconnectedGatewayAsync(_discordClient);
            DiscordClientDisposeHelper.DisposeIgnoringDisconnectedGateway(_discordClient);
        }
    }

    [Fact]
    public async Task SendReactionControlMessage_WhenTrackStartedEventRaised_SendsControlMessageAndStartsTimer()
    {
        if (!CanRun()) return;
        var client = _discordClient!;
        _reactionHandlerService.RegisterHandler(client);

        var channel = await GetTestChannelAsync(client);
        if (channel is null) return;
        var markerMessage = await channel.SendMessageAsync("e2e-reaction-control-start-" + Guid.NewGuid().ToString("N"));
        var guild = await client.GetGuildAsync(channel.GuildId!.Value);
        var wrapper = new DiscordChannelWrapper(channel, guild: guild);
        var embed = new DiscordEmbedBuilder().WithTitle("E2E track").Build();

        await _lavaLinkServiceMock.RaiseAsync(
            x => x.TrackStarted += null!,
            wrapper,
            embed);

        var controlMessage = await DiscordMessageWaiter.WaitForMessageAfterAsync(
            channel,
            markerMessage.Id,
            message => message.Content.Contains(_controlMarker, StringComparison.Ordinal),
            "reaction control message");
        Assert.Contains(_controlMarker, controlMessage.Content, StringComparison.Ordinal);

        _progressiveTimerServiceMock.Verify(
            x => x.StartAsync(It.IsAny<IDiscordMessage>(), channel.Guild.Id),
            Times.Once);

        _reactionHandlerService.UnregisterHandler(client);
    }

    public static IEnumerable<object[]> SupportedReactions()
    {
        yield return [":pause_button:"];
        yield return [":arrow_forward:"];
        yield return [":track_next:"];
        yield return [":repeat:"];
    }

    [Theory]
    [MemberData(nameof(SupportedReactions))]
    public async Task OnReactionAdded_InTestMode_CallsExpectedLavaLinkOperation(string emojiName)
    {
        if (!CanRun()) return;
        var client = _discordClient!;
        _reactionHandlerService.RegisterHandler(client);

        var channel = await GetTestChannelAsync(client);
        if (channel is null) return;
        var guild = await client.GetGuildAsync(channel.GuildId!.Value);
        var member = await guild.GetMemberAsync(client.CurrentUser.Id);
        var message = await channel.SendMessageAsync($"e2e-reaction-added-{emojiName}");
        var emoji = DiscordEmoji.FromName(client, emojiName);

        await _reactionHandlerService.HandleEventAsync(client,
            DiscordEventArgsFactory.CreateMessageReactionAdded(message, member, channel, emoji, guild));

        _lavaLinkServiceMock.Verify(x => x.PauseAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()),
            emojiName == ":pause_button:" ? Times.Once : Times.Never);
        _lavaLinkServiceMock.Verify(x => x.ResumeAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()),
            emojiName == ":arrow_forward:" ? Times.Once : Times.Never);
        _lavaLinkServiceMock.Verify(x => x.SkipAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()),
            emojiName == ":track_next:" ? Times.Once : Times.Never);
        _reactionHandlerService.UnregisterHandler(client);
    }

    [Theory]
    [MemberData(nameof(SupportedReactions))]
    public async Task OnReactionRemoved_InTestMode_CallsExpectedLavaLinkOperation(string emojiName)
    {
        if (!CanRun()) return;
        var client = _discordClient!;
        _reactionHandlerService.RegisterHandler(client);

        var channel = await GetTestChannelAsync(client);
        if (channel is null) return;
        var guild = await client.GetGuildAsync(channel.GuildId!.Value);
        var member = await guild.GetMemberAsync(client.CurrentUser.Id);
        var message = await channel.SendMessageAsync($"e2e-reaction-removed-{emojiName}");
        var emoji = DiscordEmoji.FromName(client, emojiName);

        await _reactionHandlerService.HandleEventAsync(client,
            DiscordEventArgsFactory.CreateMessageReactionRemoved(message, member, channel, emoji, guild));

        _lavaLinkServiceMock.Verify(x => x.PauseAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()),
            emojiName == ":arrow_forward:" ? Times.Once : Times.Never);
        _lavaLinkServiceMock.Verify(x => x.ResumeAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()),
            emojiName == ":pause_button:" ? Times.Once : Times.Never);
        _lavaLinkServiceMock.Verify(x => x.SkipAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()),
            emojiName == ":track_next:" ? Times.Once : Times.Never);

        _reactionHandlerService.UnregisterHandler(client);
    }

    [Theory]
    [MemberData(nameof(SupportedReactions))]
    public async Task OnReactionAdded_WhenBotAddsReaction_AndIsTestModeFalse_DoesNotCallLavaLinkOperations(string emojiName)
    {
        if (!CanRun()) return;
        var client = _discordClient!;
        _productionReactionHandlerService.RegisterHandler(client);

        var channel = await GetTestChannelAsync(client);
        if (channel is null) return;
        var guild = DiscordEventArgsFactory.CreateGuild(channel.GuildId!.Value);
        var message = await channel.SendMessageAsync($"bot-ignore-add-{emojiName}");
        var emoji = DiscordEmoji.FromName(client, emojiName);

        await _productionReactionHandlerService.HandleEventAsync(client,
            DiscordEventArgsFactory.CreateMessageReactionAdded(message, client.CurrentUser, channel, emoji, guild));

        _lavaLinkServiceMock.Verify(x => x.PauseAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()), Times.Never);
        _lavaLinkServiceMock.Verify(x => x.ResumeAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()), Times.Never);
        _lavaLinkServiceMock.Verify(x => x.SkipAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()), Times.Never);

        _productionReactionHandlerService.UnregisterHandler(client);
    }

    [Theory]
    [MemberData(nameof(SupportedReactions))]
    public async Task OnReactionRemoved_WhenBotRemovesReaction_AndIsTestModeFalse_DoesNotCallLavaLinkOperations(string emojiName)
    {
        if (!CanRun()) return;
        var client = _discordClient!;
        _productionReactionHandlerService.RegisterHandler(client);

        var channel = await GetTestChannelAsync(client);
        if (channel is null) return;
        var guild = DiscordEventArgsFactory.CreateGuild(channel.GuildId!.Value);
        var message = await channel.SendMessageAsync($"bot-ignore-remove-{emojiName}");
        var emoji = DiscordEmoji.FromName(client, emojiName);

        _lavaLinkServiceMock.Invocations.Clear();

        await _productionReactionHandlerService.HandleEventAsync(client,
            DiscordEventArgsFactory.CreateMessageReactionRemoved(message, client.CurrentUser, channel, emoji, guild));

        _lavaLinkServiceMock.Verify(x => x.PauseAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()), Times.Never);
        _lavaLinkServiceMock.Verify(x => x.ResumeAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()), Times.Never);
        _lavaLinkServiceMock.Verify(x => x.SkipAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()), Times.Never);

        _productionReactionHandlerService.UnregisterHandler(client);
    }

    [Fact]
    public async Task ReactionContextFactory_WithRealDiscordObjects_ReturnsExpectedGuildId()
    {
        if (!CanRun()) return;
        var client = _discordClient!;

        var channel = await GetTestChannelAsync(client);
        if (channel is null) return;
        var guild = await client.GetGuildAsync(channel.GuildId!.Value);
        var message = await channel.SendMessageAsync("e2e-build-context");

        var context = await new ReactionContextFactory().CreateAsync(message, client.CurrentUser, channel, guild);

        Assert.Equal(channel.Guild.Id, context.GuildId);
        Assert.NotNull(context.Member);
        Assert.NotNull(context.Message);
    }

    private async Task<DiscordChannel?> GetTestChannelAsync(DiscordClient client)
    {
        try
        {
            return await client.GetChannelAsync(_testChannelId);
        }
        catch (Exception exception) when (EndToEndDiscordGuard.IsDiscordEnvironmentUnavailable(exception))
        {
            return null;
        }
    }

    private bool CanRun()
    {
        return _isConfigured && _isDiscordAvailable && _discordClient != null;
    }
}
