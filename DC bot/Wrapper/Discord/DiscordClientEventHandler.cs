using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Persistence.GuildBotStatus;
using DC_bot.Interface.Service.Persistence.Guilds;
using DC_bot.Logging;
using DC_bot.Service.Music.Lifecycle;
using DC_bot.Service.Music.Lifecycle.Recovery;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DC_bot.Wrapper.Discord;

/// <summary>
/// Isolates event failures while maintaining gateway recovery and persisted bot voice state.
/// </summary>
/// <param name="logger">The structured logger used at this operation boundary.</param>
/// <param name="guildDataRepository">The guild data repository dependency used by this boundary.</param>
/// <param name="localizationService">The localization service dependency used by this boundary.</param>
/// <param name="lavaLinkService">The lava link service dependency used by this boundary.</param>
/// <param name="gatewayRecovery">The gateway recovery supplied for this operation.</param>
/// <param name="guildBotStatusRepository">The guild bot status repository dependency used by this boundary.</param>
public class DiscordClientEventHandler(
    ILogger<DiscordClientEventHandler> logger,
    IGuildDataRepository guildDataRepository,
    ILocalizationService localizationService,
    ILavaLinkService lavaLinkService,
    DiscordPlaybackRecoveryService gatewayRecovery,
    IGuildBotStatusRepository guildBotStatusRepository)
{
    private readonly DiscordConnectionEventLogger _connectionEventLogger = new(logger);
    private readonly DiscordVoiceEventLogger _voiceEventLogger = new(logger);

    /// <summary>
    /// Processes the Discord socket opened event inside the common event-failure boundary.
    /// </summary>
    /// <param name="sender">The sender supplied for this operation.</param>
    /// <param name="e">The event payload supplied by the Discord or Lavalink SDK.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task OnSocketOpened(DiscordClient sender, SocketOpenedEventArgs e)
    {
        return HandleEventAsync(nameof(OnSocketOpened), () =>
        {
            _connectionEventLogger.LogSocketOpened();
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Processes the Discord socket closed event inside the common event-failure boundary.
    /// </summary>
    /// <param name="sender">The sender supplied for this operation.</param>
    /// <param name="e">The event payload supplied by the Discord or Lavalink SDK.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task OnSocketClosed(DiscordClient sender, SocketClosedEventArgs e)
    {
        return HandleEventAsync(nameof(OnSocketClosed), () =>
        {
            _connectionEventLogger.LogSocketClosed(e);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Processes the Discord client ready event inside the common event-failure boundary.
    /// </summary>
    /// <param name="sender">The sender supplied for this operation.</param>
    /// <param name="e">The e supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task OnClientReady(DiscordClient sender, SessionCreatedEventArgs? e)
    {
        await HandleEventAsync(nameof(OnClientReady), async () =>
        {
            _connectionEventLogger.LogClientReady(e);
            await lavaLinkService.ConnectAsync(default);

            if (e is not null)
            {
                await gatewayRecovery.OnSessionCreatedAsync(e.GuildIds);
            }

        });
    }

    /// <summary>
    /// Processes the Discord session resumed event inside the common event-failure boundary.
    /// </summary>
    /// <param name="sender">The sender supplied for this operation.</param>
    /// <param name="e">The event payload supplied by the Discord or Lavalink SDK.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task OnSessionResumed(DiscordClient sender, SessionResumedEventArgs e)
    {
        return HandleEventAsync(nameof(OnSessionResumed), () =>
        {
            _connectionEventLogger.LogSessionResumed(e);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Processes the Discord zombied event inside the common event-failure boundary.
    /// </summary>
    /// <param name="sender">The sender supplied for this operation.</param>
    /// <param name="e">The event payload supplied by the Discord or Lavalink SDK.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task OnZombied(DiscordClient sender, ZombiedEventArgs e)
    {
        return HandleEventAsync(nameof(OnZombied), () =>
        {
            _connectionEventLogger.LogZombied(e);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Processes the Discord guild available event inside the common event-failure boundary.
    /// </summary>
    /// <param name="sender">The sender supplied for this operation.</param>
    /// <param name="e">The event payload supplied by the Discord or Lavalink SDK.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task OnGuildAvailable(DiscordClient sender, GuildAvailableEventArgs e)
    {
        await HandleEventAsync(nameof(OnGuildAvailable), async () =>
        {
            logger.DiscordClientGuildAvailable(e.Guild.Name);
            await guildDataRepository.EnsureGuildExistsAsync(e.Guild.Id, CancellationToken.None);
            localizationService.LoadLanguage(e.Guild.Id);
            await lavaLinkService.Init(e.Guild.Id, default);
        });
    }

    /// <summary>
    /// Processes the Discord voice state updated event inside the common event-failure boundary.
    /// </summary>
    /// <param name="sender">The sender supplied for this operation.</param>
    /// <param name="e">The event payload supplied by the Discord or Lavalink SDK.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task OnVoiceStateUpdated(DiscordClient sender, VoiceStateUpdatedEventArgs e)
    {
        return HandleEventAsync(nameof(OnVoiceStateUpdated), async () =>
        {
            _voiceEventLogger.LogVoiceStateUpdated(sender, e);
            await HandleEventAsync("OnVoiceStateUpdated.UpdateGuildBotStatus", () => UpdateGuildBotStatusAsync(sender, e));

            if (sender.CurrentUser?.Id == e.UserId && e.GuildId is { } guildId)
            {
                await gatewayRecovery.OnBotVoiceStateUpdatedAsync(guildId, e.After?.ChannelId ?? e.ChannelId);
            }

        });
    }

    /// <summary>
    /// Processes the Discord voice server updated event inside the common event-failure boundary.
    /// </summary>
    /// <param name="sender">The sender supplied for this operation.</param>
    /// <param name="e">The event payload supplied by the Discord or Lavalink SDK.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task OnVoiceServerUpdated(DiscordClient sender, VoiceServerUpdatedEventArgs e)
    {
        return HandleEventAsync(nameof(OnVoiceServerUpdated), () =>
        {
            _voiceEventLogger.LogVoiceServerUpdated(e);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Processes the Discord unknown event event inside the common event-failure boundary.
    /// </summary>
    /// <param name="sender">The sender supplied for this operation.</param>
    /// <param name="e">The event payload supplied by the Discord or Lavalink SDK.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task OnUnknownEvent(DiscordClient sender, UnknownEventArgs e)
    {
        return HandleEventAsync(nameof(OnUnknownEvent), () =>
        {
            _connectionEventLogger.LogUnknownEvent(e);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Invokes one Discord event callback and logs its failure without propagating it into SDK event delivery.
    /// </summary>
    /// <param name="eventName">The event name supplied for this operation.</param>
    /// <param name="handler">The handler supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    private async Task HandleEventAsync(string eventName, Func<Task> handler)
    {
        try
        {
            await handler();
        }
        catch (Exception exception)
        {
            logger.DiscordClientEventFailed(exception, eventName);
        }
    }

    /// <summary>
    /// Persists observed bot voice state while tolerating incomplete event entities.
    /// </summary>
    /// <param name="sender">The sender supplied for this operation.</param>
    /// <param name="e">The event payload supplied by the Discord or Lavalink SDK.</param>
    /// <returns>A task representing completion of this operation.</returns>
    private async Task UpdateGuildBotStatusAsync(DiscordClient sender, VoiceStateUpdatedEventArgs e)
    {
        if (sender.CurrentUser?.Id is not { } botUserId
            || e.GuildId is null or 0
            || !sender.Guilds.TryGetValue(e.GuildId.Value, out var guild))
        {
            return;
        }

        var isBotVoiceState = botUserId == e.UserId;
        var botVoiceChannel = ResolveBotVoiceChannel(sender, guild, e);

        if (botVoiceChannel is null)
        {
            if (isBotVoiceState)
            {
                await guildBotStatusRepository.MarkDisconnectedVoiceAsync(e.GuildId.Value);
            }

            return;
        }

        await guildBotStatusRepository.UpsertConnectedVoiceAsync(e.GuildId.Value, botVoiceChannel.Id, botVoiceChannel.Name, GetVoiceUserCount(botVoiceChannel));
    }

    /// <summary>
    /// Resolves the bot&apos;s channel from event data and available guild cache state.
    /// </summary>
    /// <param name="sender">The sender supplied for this operation.</param>
    /// <param name="guild">The guild supplied for this operation.</param>
    /// <param name="e">The event payload supplied by the Discord or Lavalink SDK.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    private static DiscordChannel? ResolveBotVoiceChannel(DiscordClient sender, DiscordGuild guild, VoiceStateUpdatedEventArgs e)
    {
        if (sender.CurrentUser?.Id is not { } botUserId)
        {
            return null;
        }

        if (botUserId == e.UserId)
        {
            return TryGetGuildChannel(guild, e.After.ChannelId ?? e.ChannelId);
        }

        if (!guild.Members.TryGetValue(botUserId, out var botMember))
        {
            return null;
        }

        return TryGetGuildChannel(guild, botMember?.VoiceState?.ChannelId);
    }

    /// <summary>
    /// Looks up a channel within the supplied guild&apos;s available channel state.
    /// </summary>
    /// <param name="guild">The guild supplied for this operation.</param>
    /// <param name="channelId">The channel id supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    private static DiscordChannel? TryGetGuildChannel(DiscordGuild guild, ulong? channelId)
    {
        return channelId is null or 0 ? null : guild.Channels.GetValueOrDefault(channelId.Value);
    }

    /// <summary>
    /// Counts voice-channel users represented in available Discord state.
    /// </summary>
    /// <param name="channel">The channel supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    private static int GetVoiceUserCount(DiscordChannel channel)
    {
        return channel.Users.Count;
    }
}
