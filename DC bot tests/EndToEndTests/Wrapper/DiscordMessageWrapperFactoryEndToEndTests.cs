using DC_bot.Helper.Factory;
using DC_bot.Interface.Discord;
using DSharpPlus;
using DSharpPlus.Entities;

namespace DC_bot_tests.EndToEndTests.Wrapper;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class DiscordMessageWrapperFactoryEndToEndTests : IAsyncLifetime
{
    private readonly ulong _testChannelId;
    private readonly DiscordClient? _discordClient;
    private readonly bool _isConfigured;
    private bool _isDiscordAvailable;

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
        _isDiscordAvailable = await EndToEndDiscordGuard.TryConnectAndWaitUntilReadyAsync(_discordClient);
    }

    public async Task DisposeAsync()
    {
        await EndToEndDiscordGuard.DisconnectIgnoringDisconnectedGatewayAsync(_discordClient);
        if (_discordClient != null)
        {
            DiscordClientDisposeHelper.DisposeIgnoringDisconnectedGateway(_discordClient);
        }
    }

    [Fact]
    public async Task Create_WithRealDiscordObjects_MapsCoreProperties()
    {
        if (!CanRun()) return;

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
        if (!CanRun()) return;

        var client = _discordClient!;
        var channel = await client.GetChannelAsync(_testChannelId);
        var original = $"factory-original-{Guid.NewGuid():N}";
        var responseMarker = $"factory-response-{Guid.NewGuid():N}";
        var modified = $"factory-modified-{Guid.NewGuid():N}";
        var message = await channel.SendMessageAsync(original);
        var wrapped = DiscordMessageWrapperFactory.Create(message, channel, client.CurrentUser);

        await wrapped.RespondAsync(responseMarker);
        var response = await DiscordMessageWaiter.WaitForMessageAfterAsync(
            channel,
            message.Id,
            discordMessage => discordMessage.Content.Contains(responseMarker, StringComparison.Ordinal),
            "wrapper response message",
            limit: 15);
        Assert.Contains(responseMarker, response.Content, StringComparison.Ordinal);

        await wrapped.ModifyAsync(new DiscordMessageBuilder().WithContent(modified));
        await AsyncTestWaiter.UntilAsync(
            async () =>
            {
                var refreshed = await channel.GetMessageAsync(message.Id);
                return string.Equals(modified, refreshed.Content, StringComparison.Ordinal);
            },
            "wrapper message content was not modified in time.");
    }

    private bool CanRun()
    {
        return _isConfigured && _isDiscordAvailable && _discordClient != null;
    }
}