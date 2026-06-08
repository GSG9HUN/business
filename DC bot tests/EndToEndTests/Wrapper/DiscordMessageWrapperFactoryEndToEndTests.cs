using DC_bot.Helper.Factory;
using DC_bot.Interface.Discord;
using DSharpPlus;
using DSharpPlus.Entities;
using Xunit.Sdk;

namespace DC_bot_tests.EndToEndTests.Wrapper;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class DiscordMessageWrapperFactoryEndToEndTests : IAsyncLifetime
{
    private readonly ulong _testChannelId;
    private readonly DiscordClient? _discordClient;
    private readonly bool _isConfigured;
    public DiscordMessageWrapperFactoryEndToEndTests()
    {
        var hasToken = EndToEndTestConfiguration.TryGetDiscordToken(out var token);
        var hasChannel = EndToEndTestConfiguration.TryGetDiscordChannelId(out var testChannelId);
        _testChannelId = testChannelId;

        if (hasToken && hasChannel)
        {
            _isConfigured = true;
            _discordClient = TestDiscordClientFactory.Create(
                token,
                DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents);
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
        await Task.Delay(2500);
    }
    public async Task DisposeAsync()
    {
        if (_discordClient != null)
        {
            await _discordClient.DisconnectAsync();
            _discordClient.Dispose();
        }
    }
    private void EnsureConfigured()
    {
        if (!_isConfigured || _discordClient == null)
            throw SkipException.ForSkip(EndToEndTestConfiguration.MissingDiscordTokenAndChannelMessage());
    }
    [Fact]
    public async Task Create_WithRealDiscordObjects_MapsCoreProperties()
    {
        EnsureConfigured();
        var client = _discordClient!;
        var channel = await client.GetChannelAsync(_testChannelId);
        var marker = $"factory-map-{Guid.NewGuid():N}";
        var message = await channel.SendMessageAsync(marker);
        IDiscordMessage wrapped = DiscordMessageWrapperFactory.Create(message, channel, client.CurrentUser);
        Assert.Equal(message.Id, wrapped.Id);
        Assert.Equal(message.Content, wrapped.Content);
        Assert.Equal(client.CurrentUser.Id, wrapped.Author.Id);
        Assert.Equal(channel.Id, wrapped.Channel.Id);
        Assert.Equal(message.CreationTimestamp, wrapped.CreatedAt);
    }
    [Fact]
    public async Task Create_WithRealDiscordObjects_RespondAndModifyWork()
    {
        EnsureConfigured();
        var client = _discordClient!;
        var channel = await client.GetChannelAsync(_testChannelId);
        var original = $"factory-original-{Guid.NewGuid():N}";
        var responseMarker = $"factory-response-{Guid.NewGuid():N}";
        var modified = $"factory-modified-{Guid.NewGuid():N}";
        var message = await channel.SendMessageAsync(original);
        var wrapped = DiscordMessageWrapperFactory.Create(message, channel, client.CurrentUser);
        await wrapped.RespondAsync(responseMarker);
        await Task.Delay(1000);
        var recentMessages = await channel.GetMessagesAsync(15);
        Assert.Contains(recentMessages, m => m.Content.Contains(responseMarker, StringComparison.Ordinal));
        await wrapped.ModifyAsync(new DiscordMessageBuilder().WithContent(modified));
        await Task.Delay(1000);
        var refreshed = await channel.GetMessageAsync(message.Id);
        Assert.Equal(modified, refreshed.Content);
    }
}
