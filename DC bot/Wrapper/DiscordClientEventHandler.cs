using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Logging;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DC_bot.Wrapper;

public class DiscordClientEventHandler(
    ILogger<DiscordClientEventHandler> logger,
    IGuildDataRepository guildDataRepository,
    ILocalizationService localizationService,
    ILavaLinkService lavaLinkService)
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
        return HandleEventAsync(nameof(OnVoiceStateUpdated), () =>
        {
            _voiceEventLogger.LogVoiceStateUpdated(sender, e);
            return Task.CompletedTask;
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
}
