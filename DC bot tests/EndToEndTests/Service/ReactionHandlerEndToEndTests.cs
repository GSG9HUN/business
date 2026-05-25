using System.Reflection;
using System.Runtime.CompilerServices;
using DC_bot.Constants;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Service;
using DC_bot.Wrapper;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Sdk;

namespace DC_bot_tests.EndToEndTests.Service;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class ReactionHandlerEndToEndTests : IAsyncLifetime
{
    private readonly ulong _testChannelId;
    private readonly string _controlMarker = $"MusicControl-{Guid.NewGuid():N}";
    private readonly DiscordClient? _discordClient;
    private readonly bool _isConfigured;
    private readonly Mock<ILavaLinkService> _lavaLinkServiceMock = new();
    private readonly Mock<ILocalizationService> _localizationServiceMock = new();
    private readonly Mock<ILogger<ReactionHandler>> _loggerMock = new();
    private readonly Mock<IProgressiveTimerService> _progressiveTimerServiceMock = new();
    private readonly ReactionHandler _reactionHandler;
    private readonly ReactionHandler _productionReactionHandler;

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

        _reactionHandler = new ReactionHandler(
            _lavaLinkServiceMock.Object,
            _loggerMock.Object,
            _progressiveTimerServiceMock.Object,
            _localizationServiceMock.Object, true);

        _productionReactionHandler = new ReactionHandler(
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
            _discordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = token,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents | DiscordIntents.GuildMessageReactions
            });
        }
        else
        {
            _isConfigured = false;
        }
    }

    public async Task InitializeAsync()
    {
        if (!_isConfigured || _discordClient == null) return;
        await _discordClient.ConnectAsync();
        await Task.Delay(1500);
    }

    public async Task DisposeAsync()
    {
        if (_discordClient != null)
        {
            _reactionHandler.UnregisterHandler(_discordClient);
            _productionReactionHandler.UnregisterHandler(_discordClient);
            await _discordClient.DisconnectAsync();
            _discordClient.Dispose();
        }
    }

    [Fact]
    public async Task SendReactionControlMessage_WhenTrackStartedEventRaised_SendsControlMessageAndStartsTimer()
    {
        EnsureConfigured();
        var client = _discordClient!;
        _reactionHandler.RegisterHandler(client);

        var channel = await client.GetChannelAsync(_testChannelId);
        var wrapper = new DiscordChannelWrapper(channel);
        var embed = new DiscordEmbedBuilder().WithTitle("E2E track").Build();

        await _lavaLinkServiceMock.RaiseAsync(
            x => x.TrackStarted += null!,
            wrapper,
            client,
            embed);

        await Task.Delay(2000);

        var recentMessages = await channel.GetMessagesAsync(10);
        Assert.Contains(recentMessages, m => m.Content.Contains(_controlMarker, StringComparison.Ordinal));

        _progressiveTimerServiceMock.Verify(
            x => x.StartAsync(It.IsAny<IDiscordMessage>(), channel.Guild.Id),
            Times.Once);

        _reactionHandler.UnregisterHandler(client);
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
        EnsureConfigured();
        var client = _discordClient!;
        _reactionHandler.RegisterHandler(client);

        var channel = await client.GetChannelAsync(_testChannelId);
        var message = await channel.SendMessageAsync($"e2e-reaction-added-{emojiName}");
        await message.CreateReactionAsync(DiscordEmoji.FromName(client, emojiName));

        await Task.Delay(1200);

        _lavaLinkServiceMock.Verify(x => x.PauseAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()),
            emojiName == ":pause_button:" ? Times.Once : Times.Never);
        _lavaLinkServiceMock.Verify(x => x.ResumeAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()),
            emojiName == ":arrow_forward:" ? Times.Once : Times.Never);
        _lavaLinkServiceMock.Verify(x => x.SkipAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()),
            emojiName == ":track_next:" ? Times.Once : Times.Never);
        _reactionHandler.UnregisterHandler(client);
    }

    [Theory]
    [MemberData(nameof(SupportedReactions))]
    public async Task OnReactionRemoved_InTestMode_CallsExpectedLavaLinkOperation(string emojiName)
    {
        EnsureConfigured();
        var client = _discordClient!;
        _reactionHandler.RegisterHandler(client);

        var channel = await client.GetChannelAsync(_testChannelId);
        var message = await channel.SendMessageAsync($"e2e-reaction-removed-{emojiName}");
        var emoji = DiscordEmoji.FromName(client, emojiName);

        await message.CreateReactionAsync(emoji);
        await Task.Delay(700);

        _lavaLinkServiceMock.Invocations.Clear();

        await message.DeleteOwnReactionAsync(emoji);

        await Task.Delay(1200);

        _lavaLinkServiceMock.Verify(x => x.PauseAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()),
            emojiName == ":arrow_forward:" ? Times.Once : Times.Never);
        _lavaLinkServiceMock.Verify(x => x.ResumeAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()),
            emojiName == ":pause_button:" ? Times.Once : Times.Never);
        _lavaLinkServiceMock.Verify(x => x.SkipAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()),
            emojiName == ":track_next:" ? Times.Once : Times.Never);

        _reactionHandler.UnregisterHandler(client);
    }

    [Theory]
    [MemberData(nameof(SupportedReactions))]
    public async Task OnReactionAdded_WhenBotAddsReaction_AndIsTestModeFalse_DoesNotCallLavaLinkOperations(string emojiName)
    {
        EnsureConfigured();
        var client = _discordClient!;
        _productionReactionHandler.RegisterHandler(client);

        var channel = await client.GetChannelAsync(_testChannelId);
        var message = await channel.SendMessageAsync($"bot-ignore-add-{emojiName}");
        await message.CreateReactionAsync(DiscordEmoji.FromName(client, emojiName));

        await Task.Delay(1200);

        _lavaLinkServiceMock.Verify(x => x.PauseAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()), Times.Never);
        _lavaLinkServiceMock.Verify(x => x.ResumeAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()), Times.Never);
        _lavaLinkServiceMock.Verify(x => x.SkipAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()), Times.Never);

        _productionReactionHandler.UnregisterHandler(client);
    }

    [Theory]
    [MemberData(nameof(SupportedReactions))]
    public async Task OnReactionRemoved_WhenBotRemovesReaction_AndIsTestModeFalse_DoesNotCallLavaLinkOperations(string emojiName)
    {
        EnsureConfigured();
        var client = _discordClient!;
        _productionReactionHandler.RegisterHandler(client);

        var channel = await client.GetChannelAsync(_testChannelId);
        var message = await channel.SendMessageAsync($"bot-ignore-remove-{emojiName}");
        var emoji = DiscordEmoji.FromName(client, emojiName);

        await message.CreateReactionAsync(emoji);
        await Task.Delay(700);
        _lavaLinkServiceMock.Invocations.Clear();

        await message.DeleteOwnReactionAsync(emoji);
        await Task.Delay(1200);

        _lavaLinkServiceMock.Verify(x => x.PauseAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()), Times.Never);
        _lavaLinkServiceMock.Verify(x => x.ResumeAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()), Times.Never);
        _lavaLinkServiceMock.Verify(x => x.SkipAsync(It.IsAny<IDiscordMessage>(), It.IsAny<IDiscordMember?>()), Times.Never);

        _productionReactionHandler.UnregisterHandler(client);
    }

    [Fact]
    public async Task BuildContextAsync_WithRealDiscordObjects_ReturnsExpectedGuildId()
    {
        EnsureConfigured();
        var client = _discordClient!;

        var channel = await client.GetChannelAsync(_testChannelId);
        var message = await channel.SendMessageAsync("e2e-build-context");

        var method = typeof(ReactionHandler).GetMethod("BuildContextAsync", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var task = (Task)method.Invoke(null, [message, client.CurrentUser, channel])!;
        await task;

        var resultProperty = task.GetType().GetProperty("Result");
        Assert.NotNull(resultProperty);

        var result = resultProperty.GetValue(task);
        Assert.NotNull(result);

        var tuple = (ITuple)result;
        Assert.Equal(channel.Guild.Id, (ulong)tuple[2]!);
        Assert.NotNull(tuple[0]);
        Assert.NotNull(tuple[1]);
    }

    private void EnsureConfigured()
    {
        if (!_isConfigured || _discordClient == null)
            throw SkipException.ForSkip(EndToEndTestConfiguration.MissingDiscordTokenAndChannelMessage());
    }
}
