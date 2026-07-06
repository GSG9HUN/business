using DC_bot.Interface.Discord;
using DC_bot.Wrapper;
using DSharpPlus;
using DSharpPlus.Entities;

namespace DC_bot_tests.EndToEndTests.Wrapper;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class DiscordChannelWrapperEndToEndTests : IAsyncLifetime
{
    private readonly DiscordClient? _discordClient;
    private readonly bool _isConfigured;
    private readonly ulong _guildId;
    private readonly ulong _testChannelId;
    private bool _isDiscordAvailable;

    public DiscordChannelWrapperEndToEndTests()
    {
        var hasToken = EndToEndTestConfiguration.TryGetDiscordToken(out var token);
        var hasGuild = EndToEndTestConfiguration.TryGetDiscordGuildId(out var guildId);
        var hasChannel = EndToEndTestConfiguration.TryGetDiscordChannelId(out var testChannelId);

        _guildId = guildId;
        _testChannelId = testChannelId;

        if (hasToken && hasGuild && hasChannel)
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

    private DiscordChannel? GetTestChannel()
    {
        if (!CanRun()) return null;
        if (!_discordClient!.Guilds.TryGetValue(_guildId, out var guild)) return null;
        return guild.Channels.GetValueOrDefault(_testChannelId);
    }

    [Fact]
    public void Id_ReturnsCorrectChannelId()
    {
        var channel = GetTestChannel();
        if (channel is null) return;

        var wrapper = new DiscordChannelWrapper(channel);

        Assert.Equal(_testChannelId, wrapper.Id);
    }

    [Fact]
    public void Name_ReturnsNonEmptyChannelName()
    {
        var channel = GetTestChannel();
        if (channel is null) return;

        var wrapper = new DiscordChannelWrapper(channel);

        Assert.NotEmpty(wrapper.Name);
        Assert.Equal(channel.Name, wrapper.Name);
    }

    [Fact]
    public void ToDiscordChannel_ReturnsSameInstance()
    {
        var channel = GetTestChannel();
        if (channel is null) return;

        var wrapper = new DiscordChannelWrapper(channel);

        Assert.Equal(channel, wrapper.ToDiscordChannel());
    }

    [Fact]
    public void Guild_ReturnsDiscordGuildWrapper()
    {
        var channel = GetTestChannel();
        if (channel is null) return;

        var wrapper = new DiscordChannelWrapper(channel);

        Assert.IsType<DiscordGuildWrapper>(wrapper.Guild);
        Assert.IsAssignableFrom<IDiscordGuild>(wrapper.Guild);
    }

    [Fact]
    public void Guild_ReturnsCorrectGuildId()
    {
        var channel = GetTestChannel();
        if (channel is null) return;

        var wrapper = new DiscordChannelWrapper(channel);

        Assert.Equal(_guildId, wrapper.Guild.Id);
    }

    [Fact]
    public async Task SendMessageAsync_String_DoesNotThrow()
    {
        var channel = GetTestChannel();
        if (channel is null) return;

        var wrapper = new DiscordChannelWrapper(channel);

        await wrapper.SendMessageAsync($"E2E test message - string - {Guid.NewGuid():N}");
    }

    [Fact]
    public async Task SendMessageAsync_Embed_DoesNotThrow()
    {
        var channel = GetTestChannel();
        if (channel is null) return;

        var embed = new DiscordEmbedBuilder()
            .WithTitle("E2E Test")
            .WithDescription($"DiscordChannelWrapper E2E test - embed - {Guid.NewGuid():N}")
            .Build();

        var wrapper = new DiscordChannelWrapper(channel);

        await wrapper.SendMessageAsync(embed);
    }

    private bool CanRun()
    {
        return _isConfigured && _isDiscordAvailable && _discordClient != null;
    }
}