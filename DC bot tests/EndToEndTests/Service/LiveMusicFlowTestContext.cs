using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music;
using DSharpPlus.Entities;
using Lavalink4NET.Players;
using Lavalink4NET.Protocol.Payloads.Events;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace DC_bot_tests.EndToEndTests.Service;

internal sealed class LiveMusicFlowTestContext : IAsyncDisposable
{
    private readonly DiscordE2EClientFixture _discordFixture;
    private readonly MusicFlowDriver _driver;
    private readonly LavalinkE2EFixture _lavalinkFixture;
    private readonly LiveDiscordMessageProbe _messageProbe;

    private LiveMusicFlowTestContext(
        DiscordE2EClientFixture discordFixture,
        MusicFlowDriver driver,
        LavalinkE2EFixture lavalinkFixture,
        LiveDiscordMessageProbe messageProbe)
    {
        _discordFixture = discordFixture;
        _driver = driver;
        _lavalinkFixture = lavalinkFixture;
        _messageProbe = messageProbe;
    }

    public ServiceProvider Provider => _discordFixture.Provider;
    public DiscordGuild Guild => _discordFixture.Guild;
    public ulong GuildId => _discordFixture.GuildId;
    public DiscordChannel TextChannel => _discordFixture.TextChannel;
    public DiscordChannel VoiceChannel => _discordFixture.VoiceChannel;
    public DiscordMessage TestRunMarker => _discordFixture.TestRunMarker;
    public LiveMusicFlowMessage Message => _driver.Message;
    public IDiscordMember Member => _driver.Member;

    public static async Task<LiveMusicFlowTestContext?> TryCreateAsync(ITestOutputHelper testOutputHelper)
    {
        var discordFixture = await DiscordE2EClientFixture.TryCreateAsync(testOutputHelper);
        if (discordFixture is null)
        {
            return null;
        }

        try
        {
            var driver = MusicFlowDriver.Create(discordFixture, testOutputHelper);
            var lavalinkFixture = new LavalinkE2EFixture(
                discordFixture.Provider,
                discordFixture.GuildId,
                driver.Message,
                driver.ChannelMessages);
            var messageProbe = new LiveDiscordMessageProbe(
                discordFixture.TextChannel,
                discordFixture.TestRunMarker);

            return new LiveMusicFlowTestContext(
                discordFixture,
                driver,
                lavalinkFixture,
                messageProbe);
        }
        catch
        {
            await discordFixture.DisposeAsync();
            throw;
        }
    }

    public Task ExecuteCommandAsync(string commandName, string commandText)
    {
        return _driver.ExecuteCommandAsync(commandName, commandText);
    }

    public async Task LeaveAsync()
    {
        await _driver.LeaveAsync();
        await _lavalinkFixture.WaitForPlayerWithoutCurrentTrackAsync();
    }

    public Task<ILavalinkPlayer> WaitForPlayerWithCurrentTrackAsync()
    {
        return _lavalinkFixture.WaitForPlayerWithCurrentTrackAsync();
    }

    public Task WaitForPlayerWithoutCurrentTrackAsync()
    {
        return _lavalinkFixture.WaitForPlayerWithoutCurrentTrackAsync();
    }

    public Task<ILavaLinkTrack> WaitForStoredCurrentTrackAsync(Func<ILavaLinkTrack, bool>? predicate = null)
    {
        return _lavalinkFixture.WaitForStoredCurrentTrackAsync(predicate);
    }

    public Task<ILavaLinkTrack> WaitForQueuedTrackAsync()
    {
        return _lavalinkFixture.WaitForQueuedTrackAsync();
    }

    public Task SimulateTrackEndedAsync(TrackEndReason reason)
    {
        return _lavalinkFixture.SimulateTrackEndedAsync(reason);
    }

    public Task ExecuteReactionAddedAsync(string emojiName)
    {
        return _driver.ExecuteReactionAddedAsync(emojiName);
    }

    public Task<DiscordMessage> WaitForMusicControlMessageAsync()
    {
        return _messageProbe.WaitForMusicControlMessageAsync();
    }

    public Task<DiscordMessage> WaitForControlMessageDescriptionChangeAsync(
        ulong messageId,
        string initialDescription)
    {
        return _messageProbe.WaitForControlMessageDescriptionChangeAsync(messageId, initialDescription);
    }

    public async ValueTask DisposeAsync()
    {
        if (!_driver.LeaveExecuted)
        {
            await _driver.TryLeaveForCleanupAsync();
        }

        await _discordFixture.DisposeAsync();
    }
}
