using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Interface.Service.Persistence.Guilds;
using DC_bot.Interface.Service.Persistence.GuildBotStatus;
using DC_bot.Logging;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DC_bot.Wrapper;

public class DiscordClientEventHandler(
    ILogger<DiscordClientEventHandler> logger,
    IGuildDataRepository guildDataRepository,
    ILocalizationService localizationService,
    ILavaLinkService lavaLinkService,
    IGuildBotStatusRepository guildBotStatusRepository)
{
    private readonly DiscordConnectionEventLogger _connectionEventLogger = new(logger);
    private readonly DiscordVoiceEventLogger _voiceEventLogger = new(logger);

    public Task OnSocketOpened(DiscordClient sender, SocketOpenedEventArgs e)
    {
        return HandleEventAsync(nameof(OnSocketOpened), () =>
        {
            _connectionEventLogger.LogSocketOpened();
            return Task.CompletedTask;
        });
    }

    public Task OnSocketClosed(DiscordClient sender, SocketClosedEventArgs e)
    {
        return HandleEventAsync(nameof(OnSocketClosed), () =>
        {
            _connectionEventLogger.LogSocketClosed(e);
            return Task.CompletedTask;
        });
    }

    public async Task OnClientReady(DiscordClient sender, SessionCreatedEventArgs? e)
    {
        await HandleEventAsync(nameof(OnClientReady), async () =>
        {
            _connectionEventLogger.LogClientReady(e);
            await lavaLinkService.ConnectAsync();
        });
    }

    public Task OnSessionResumed(DiscordClient sender, SessionResumedEventArgs e)
    {
        return HandleEventAsync(nameof(OnSessionResumed), () =>
        {
            _connectionEventLogger.LogSessionResumed(e);
            return Task.CompletedTask;
        });
    }

    public Task OnZombied(DiscordClient sender, ZombiedEventArgs e)
    {
        return HandleEventAsync(nameof(OnZombied), () =>
        {
            _connectionEventLogger.LogZombied(e);
            return Task.CompletedTask;
        });
    }

    public async Task OnGuildAvailable(DiscordClient sender, GuildAvailableEventArgs e)
    {
        await HandleEventAsync(nameof(OnGuildAvailable), async () =>
        {
            logger.DiscordClientGuildAvailable(e.Guild.Name);

            await guildDataRepository.EnsureGuildExistsAsync(e.Guild.Id, CancellationToken.None);
            localizationService.LoadLanguage(e.Guild.Id);
            await lavaLinkService.Init(e.Guild.Id);
        });
    }

    public Task OnVoiceStateUpdated(DiscordClient sender, VoiceStateUpdatedEventArgs e)
    {
        return HandleEventAsync(nameof(OnVoiceStateUpdated), async () =>
        {
            _voiceEventLogger.LogVoiceStateUpdated(sender, e);
            await UpdateGuildBotStatusAsync(sender, e);
        });
    }

    public Task OnVoiceServerUpdated(DiscordClient sender, VoiceServerUpdatedEventArgs e)
    {
        return HandleEventAsync(nameof(OnVoiceServerUpdated), () =>
        {
            _voiceEventLogger.LogVoiceServerUpdated(e);
            return Task.CompletedTask;
        });
    }

    public Task OnUnknownEvent(DiscordClient sender, UnknownEventArgs e)
    {
        return HandleEventAsync(nameof(OnUnknownEvent), () =>
        {
            _connectionEventLogger.LogUnknownEvent(e);
            return Task.CompletedTask;
        });
    }

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

    private async Task UpdateGuildBotStatusAsync(DiscordClient sender, VoiceStateUpdatedEventArgs e)
    {
        if (e.GuildId is null or 0 || !sender.Guilds.TryGetValue(e.GuildId.Value, out var guild))
        {
            return;
        }

        var isBotVoiceState = sender.CurrentUser.Id == e.UserId;
        var botVoiceChannel = ResolveBotVoiceChannel(sender, guild, e);

        if (botVoiceChannel is null)
        {
            if (isBotVoiceState)
            {
                await guildBotStatusRepository.MarkDisconnectedVoiceAsync(e.GuildId.Value);
            }

            return;
        }

        await guildBotStatusRepository.UpsertConnectedVoiceAsync(
            e.GuildId.Value,
            botVoiceChannel.Id,
            botVoiceChannel.Name,
            GetVoiceUserCount(botVoiceChannel));
    }

    private static DiscordChannel? ResolveBotVoiceChannel(
        DiscordClient sender,
        DiscordGuild guild,
        VoiceStateUpdatedEventArgs e)
    {
        if (sender.CurrentUser.Id == e.UserId)
        {
            return TryGetGuildChannel(guild, e.After?.ChannelId ?? e.ChannelId);
        }

        if (!guild.Members.TryGetValue(sender.CurrentUser.Id, out var botMember))
        {
            return null;
        }

        return TryGetGuildChannel(guild, botMember.VoiceState?.ChannelId);
    }

    private static DiscordChannel? TryGetGuildChannel(DiscordGuild guild, ulong? channelId)
    {
        if (channelId is null or 0)
        {
            return null;
        }

        return guild.Channels.TryGetValue(channelId.Value, out var channel) ? channel : null;
    }

    private static int GetVoiceUserCount(DiscordChannel channel)
    {
        return channel.Users?.Count ?? 0;
    }
}
