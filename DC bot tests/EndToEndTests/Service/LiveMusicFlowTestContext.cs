using DC_bot.Configuration;
using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Service;
using DC_bot.Startup;
using DC_bot_tests.IntegrationTests.Persistence;
using DC_bot.Service.ReactionHandler;
using DSharpPlus;
using DSharpPlus.Entities;
using Lavalink4NET;
using Lavalink4NET.Events.Players;
using Lavalink4NET.Players;
using Lavalink4NET.Protocol.Payloads.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace DC_bot_tests.EndToEndTests.Service;

internal sealed class LiveMusicFlowTestContext : IAsyncDisposable
{
    private readonly PostgreSqlTestDatabase _database;
    private readonly DiscordClient _client;
    private readonly ReactionHandlerService _reactionHandlerService;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IReadOnlyCollection<string> _channelMessages;

    private bool _leaveExecuted;

    private LiveMusicFlowTestContext(
        PostgreSqlTestDatabase database,
        ServiceProvider provider,
        DiscordClient client,
        ReactionHandlerService reactionHandlerService,
        DiscordGuild guild,
        DiscordChannel textChannel,
        DiscordChannel voiceChannel,
        DiscordMessage testRunMarker,
        LiveMusicFlowMessage message,
        IReadOnlyCollection<string> channelMessages,
        IDiscordMember member,
        ITestOutputHelper testOutputHelper)
    {
        _database = database;
        Provider = provider;
        _client = client;
        _reactionHandlerService = reactionHandlerService;
        Guild = guild;
        TextChannel = textChannel;
        VoiceChannel = voiceChannel;
        TestRunMarker = testRunMarker;
        Message = message;
        _channelMessages = channelMessages;
        Member = member;
        _testOutputHelper = testOutputHelper;
    }

    public ServiceProvider Provider { get; }
    public DiscordGuild Guild { get; }
    public ulong GuildId => Guild.Id;
    public DiscordChannel TextChannel { get; }
    public DiscordChannel VoiceChannel { get; }
    public DiscordMessage TestRunMarker { get; }
    public LiveMusicFlowMessage Message { get; }
    public IDiscordMember Member { get; }

    public static async Task<LiveMusicFlowTestContext?> TryCreateAsync(ITestOutputHelper testOutputHelper)
    {
        var hasToken = EndToEndTestConfiguration.TryGetDiscordToken(out var token);
        var hasGuild = EndToEndTestConfiguration.TryGetDiscordGuildId(out var guildId);
        var hasTextChannel = EndToEndTestConfiguration.TryGetDiscordChannelId(out var textChannelId);
        var hasVoiceChannel = EndToEndTestConfiguration.TryGetDiscordVoiceChannelId(out var voiceChannelId);
        var hasLavalink = EndToEndTestConfiguration.TryGetLavalinkSettings(out var lavalinkSettings);
        if (!hasToken || !hasGuild || !hasTextChannel || !hasVoiceChannel || !hasLavalink)
        {
            testOutputHelper.WriteLine(EndToEndTestConfiguration.MissingMusicFlowMessage());
            return null;
        }

        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null)
        {
            testOutputHelper.WriteLine("E2E music flow test requires Docker for PostgreSQL.");
            return null;
        }

        var provider = BotServiceProviderFactory.Create(
            new BotSettings { Token = token, Prefix = "!" },
            lavalinkSettings,
            database.ConnectionString);

        try
        {
            await DatabaseMigrationRunner.ApplyMigrationsIfNeededAsync(provider);

            var client = provider.GetRequiredService<DiscordClient>();
            await provider.GetRequiredService<BotService>().StartAsync(isTestEnvironment: true);
            await provider.GetRequiredService<ILavaLinkService>().ConnectAsync();

            var reactionHandler = provider.GetRequiredService<ReactionHandlerService>();
            reactionHandler.RegisterHandler(client);

            var discordGuild = await client.GetGuildAsync(guildId);
            var discordTextChannel = await client.GetChannelAsync(textChannelId);
            var discordVoiceChannel = await client.GetChannelAsync(voiceChannelId);

            Assert.Equal(guildId, discordGuild.Id);
            Assert.Equal(textChannelId, discordTextChannel.Id);
            Assert.Equal(voiceChannelId, discordVoiceChannel.Id);
            testOutputHelper.WriteLine(
                $"Music flow target voice channel: Id={discordVoiceChannel.Id}, Name={discordVoiceChannel.Name}, Type={discordVoiceChannel.Type}");

            var testRunMarker = await discordTextChannel.SendMessageAsync(
                $"E2E music flow started: {Guid.NewGuid():N}");

            var testContext = CreateCommandContext(
                discordGuild,
                guildId,
                discordTextChannel,
                voiceChannelId,
                discordVoiceChannel);

            return new LiveMusicFlowTestContext(
                database,
                provider,
                client,
                reactionHandler,
                discordGuild,
                discordTextChannel,
                discordVoiceChannel,
                testRunMarker,
                testContext.Message,
                testContext.ChannelMessages,
                testContext.Member,
                testOutputHelper);
        }
        catch
        {
            await ServiceProviderDisposeHelper.DisposeIgnoringDisconnectedDiscordClientAsync(provider);
            await database.DisposeAsync();
            throw;
        }
    }

    public async Task ExecuteCommandAsync(string commandName, string commandText)
    {
        await TextChannel.SendMessageAsync($"E2E executing: `{commandText}`");
        Message.Content = commandText;
        await GetCommand(Provider, commandName).ExecuteAsync(Message);
        WriteDiagnostics($"after {commandText}");
    }

    public async Task LeaveAsync()
    {
        await ExecuteCommandAsync("leave", "!leave");
        _leaveExecuted = true;
        await WaitForPlayerWithoutCurrentTrackAsync();
    }

    public async Task<ILavalinkPlayer> WaitForPlayerWithCurrentTrackAsync()
    {
        var audioService = Provider.GetRequiredService<IAudioService>();
        ILavalinkPlayer? lastPlayer = null;

        return await AsyncTestWaiter.UntilNotNullAsync(
            async () =>
            {
                var player = await audioService.Players.GetPlayerAsync(GuildId);
                lastPlayer = player;
                return player?.CurrentTrack is null ? null : player;
            },
            () => "Lavalink player did not start a current track in time. " +
                  "PlayerPresent=" + (lastPlayer is not null) + "; " +
                  "ConnectionState=" + (lastPlayer?.ConnectionState.ToString() ?? "none") + "; " +
                  "TextResponses=[" + string.Join(" | ", Message.TextResponses) + "]; " +
                  "ChannelMessages=[" + string.Join(" | ", _channelMessages) + "]");
    }

    public async Task WaitForPlayerWithoutCurrentTrackAsync()
    {
        var audioService = Provider.GetRequiredService<IAudioService>();

        await AsyncTestWaiter.UntilAsync(
            async () =>
            {
                var player = await audioService.Players.GetPlayerAsync(GuildId);
                return player?.CurrentTrack is null;
            },
            "Lavalink player did not stop the current track in time.");
    }

    public async Task<ILavaLinkTrack> WaitForStoredCurrentTrackAsync(
        Func<ILavaLinkTrack, bool>? predicate = null)
    {
        var currentTrackService = Provider.GetRequiredService<ICurrentTrackService>();

        return await AsyncTestWaiter.UntilNotNullAsync(
            async () =>
            {
                var track = await currentTrackService.GetCurrentTrackAsync(GuildId);
                return track is not null && (predicate is null || predicate(track)) ? track : null;
            },
            "Current track state was not persisted in time.");
    }

    public async Task<ILavaLinkTrack> WaitForQueuedTrackAsync()
    {
        var queueService = Provider.GetRequiredService<IMusicQueueService>();

        return await AsyncTestWaiter.UntilNotNullAsync(
            async () =>
            {
                var queuedTracks = await queueService.ViewQueue(GuildId);
                return queuedTracks.Count > 0 ? queuedTracks.First() : null;
            },
            "Expected queued track was not persisted in time.");
    }

    public async Task SimulateTrackEndedAsync(TrackEndReason reason)
    {
        var player = await WaitForPlayerWithCurrentTrackAsync();
        var currentTrack = player.CurrentTrack ?? throw new InvalidOperationException("No current Lavalink track.");
        var args = new TrackEndedEventArgs(player, currentTrack, reason);

        await Provider.GetRequiredService<ITrackEndedHandlerService>()
            .HandleTrackEndedAsync(player, args, Message.Channel);
    }

    public async Task ExecuteReactionAddedAsync(string emojiName)
    {
        var handler = new ReactionHandlerService(
            Provider.GetRequiredService<ILavaLinkService>(),
            Provider.GetRequiredService<ILogger<ReactionHandlerService>>(),
            Provider.GetRequiredService<IProgressiveTimerService>(),
            Provider.GetRequiredService<ILocalizationService>(),
            isTestMode: true);

        handler.RegisterHandler(_client);
        try
        {
            var discordMember = await Guild.GetMemberAsync(_client.CurrentUser.Id);
            var message = await TextChannel.SendMessageAsync($"e2e-reaction-control-{emojiName}");
            var emoji = DiscordEmoji.FromName(_client, emojiName);
            var args = DiscordEventArgsFactory.CreateMessageReactionAdded(
                message,
                discordMember,
                TextChannel,
                emoji,
                Guild);

            await handler.HandleEventAsync(_client, args);
        }
        finally
        {
            handler.UnregisterHandler(_client);
        }
    }

    public async Task<DiscordMessage> WaitForMusicControlMessageAsync()
    {
        return await AsyncTestWaiter.UntilNotNullAsync(
            async () =>
            {
                var messages = await TextChannel.GetMessagesAfterAsync(TestRunMarker.Id, 20);
                return messages
                    .OrderBy(message => message.CreationTimestamp)
                    .FirstOrDefault(message => message.Embeds.Count > 0);
            },
            "Music flow E2E did not publish a now-playing control message to Discord chat.");
    }

    public async Task<DiscordMessage> WaitForControlMessageDescriptionChangeAsync(
        ulong messageId,
        string initialDescription)
    {
        return await AsyncTestWaiter.UntilNotNullAsync(
            async () =>
            {
                var updatedMessage = await TextChannel.GetMessageAsync(messageId);
                var updatedDescription = updatedMessage.Embeds.FirstOrDefault()?.Description ?? string.Empty;
                return string.Equals(updatedDescription, initialDescription, StringComparison.Ordinal)
                    ? null
                    : updatedMessage;
            },
            "Music flow E2E control message progress did not update in time.");
    }

    public async ValueTask DisposeAsync()
    {
        _reactionHandlerService.UnregisterHandler(_client);

        if (!_leaveExecuted)
        {
            try
            {
                Message.Content = "!leave";
                await GetCommand(Provider, "leave").ExecuteAsync(Message);
            }
            catch
            {
                // Best-effort cleanup after an E2E playback failure.
            }
        }

        await ServiceProviderDisposeHelper.DisposeIgnoringDisconnectedDiscordClientAsync(Provider);
        await _database.DisposeAsync();
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
            $"{phase}: TextResponses=[{string.Join(" | ", Message.TextResponses)}], ChannelMessages=[{string.Join(" | ", _channelMessages)}], Embeds={Message.EmbedResponses.Count}");
    }

    private sealed record LiveCommandContext(
        LiveMusicFlowMessage Message,
        IReadOnlyCollection<string> ChannelMessages,
        IDiscordMember Member);
}

internal sealed class LiveMusicFlowMessage(IDiscordChannel channel, IDiscordUser user) : IDiscordMessage
{
    public ulong Id { get; set; } = 1;
    public string Content { get; set; } = "";
    public IDiscordChannel Channel { get; set; } = channel;
    public IDiscordUser Author { get; set; } = user;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public IReadOnlyList<DiscordEmbed> Embeds { get; set; } = [];
    public List<string> TextResponses { get; } = [];
    public List<DiscordEmbed> EmbedResponses { get; } = [];

    public Task RespondAsync(string message)
    {
        TextResponses.Add(message);
        return Channel.SendMessageAsync(message);
    }

    public Task RespondAsync(DiscordEmbed message)
    {
        EmbedResponses.Add(message);
        return Channel.SendMessageAsync(message);
    }

    public Task ModifyAsync(DiscordMessageBuilder builder)
    {
        if (!string.IsNullOrWhiteSpace(builder.Content))
        {
            TextResponses.Add(builder.Content);
        }

        foreach (var embed in builder.Embeds)
        {
            EmbedResponses.Add(embed);
        }

        return Task.CompletedTask;
    }
}
