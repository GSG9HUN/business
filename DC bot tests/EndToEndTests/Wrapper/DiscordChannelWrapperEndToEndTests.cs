using DC_bot.Interface.Discord;
using DC_bot.Wrapper;
using DSharpPlus;
using DSharpPlus.Entities;
using Xunit.Sdk;

namespace DC_bot_tests.EndToEndTests.Wrapper;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class DiscordChannelWrapperEndToEndTests : IAsyncLifetime
{
    private readonly DiscordClient? _discordClient;
    private readonly bool _isConfigured;
    private readonly ulong _guildId;
    private readonly ulong _testChannelId;

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
        await _discordClient.ConnectAsync();
        await Task.Delay(3000);
    }

    public async Task DisposeAsync()
    {
        if (_discordClient != null)
        {
            await _discordClient.DisconnectAsync();
        }
    }

    private DiscordChannel? GetTestChannel()
    {
        if (_discordClient == null) return null;
        if (!_discordClient.Guilds.TryGetValue(_guildId, out var guild)) return null;
        return guild.Channels.GetValueOrDefault(_testChannelId);
    }

    private DiscordChannel GetRequiredTestChannel()
    {
        var channel = GetTestChannel();
        if (channel is null)
            throw SkipException.ForSkip($"E2E test channel '{_testChannelId}' not found in guild '{_guildId}'.");
        return channel;
    }

    [Fact]
    public void Id_ReturnsCorrectChannelId()
    {
        EnsureConfigured();

        var channel = GetRequiredTestChannel();
        var wrapper = new DiscordChannelWrapper(channel);

        Assert.Equal(_testChannelId, wrapper.Id);
    }

    [Fact]
    public void Name_ReturnsNonEmptyChannelName()
    {
        EnsureConfigured();

        var channel = GetRequiredTestChannel();
        var wrapper = new DiscordChannelWrapper(channel);

        Assert.NotEmpty(wrapper.Name);
        Assert.Equal(channel.Name, wrapper.Name);
    }

    [Fact]
    public void ToDiscordChannel_ReturnsSameInstance()
    {
        EnsureConfigured();

        var channel = GetRequiredTestChannel();
        var wrapper = new DiscordChannelWrapper(channel);

        Assert.Equal(channel, wrapper.ToDiscordChannel());
    }

    [Fact]
    public void Guild_ReturnsDiscordGuildWrapper()
    {
        EnsureConfigured();

        var channel = GetRequiredTestChannel();
        var wrapper = new DiscordChannelWrapper(channel);

        Assert.IsType<DiscordGuildWrapper>(wrapper.Guild);
        Assert.IsAssignableFrom<IDiscordGuild>(wrapper.Guild);
    }

    [Fact]
    public void Guild_ReturnsCorrectGuildId()
    {
        EnsureConfigured();

        var channel = GetRequiredTestChannel();
        var wrapper = new DiscordChannelWrapper(channel);

        Assert.Equal(_guildId, wrapper.Guild.Id);
    }

    [Fact]
    public async Task SendMessageAsync_String_DoesNotThrow()
    {
        EnsureConfigured();

        var channel = GetRequiredTestChannel();
        var wrapper = new DiscordChannelWrapper(channel);

        await wrapper.SendMessageAsync($"E2E test message - string - {Guid.NewGuid():N}");
    }

    [Fact]
    public async Task SendMessageAsync_Embed_DoesNotThrow()
    {
        EnsureConfigured();

        var channel = GetRequiredTestChannel();
        var embed = new DiscordEmbedBuilder()
            .WithTitle("E2E Test")
            .WithDescription($"DiscordChannelWrapper E2E test - embed - {Guid.NewGuid():N}")
            .Build();

        var wrapper = new DiscordChannelWrapper(channel);

        await wrapper.SendMessageAsync(embed);
    }

    private void EnsureConfigured()
    {
        if (!_isConfigured || _discordClient == null)
        {
            throw SkipException.ForSkip(EndToEndTestConfiguration.MissingDiscordTokenGuildAndChannelMessage());
        }
    }
}
