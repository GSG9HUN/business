using DC_bot.Interface.Discord;
using DC_bot.Wrapper;
using DotNetEnv;
using DSharpPlus;
using DSharpPlus.Entities; 
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Sdk;

namespace DC_bot_tests.IntegrationTests.Wrapper;

[Collection("Integration Tests")]
public class DiscordChannelWrapperIntegrationTests : IAsyncLifetime
{
    private readonly DiscordClient? _discordClient;
    private readonly bool _isConfigured;
    private readonly ulong _guildId = 1309813939563003966;
    private const ulong TestChannelId = 1339151008307351572;

    public DiscordChannelWrapperIntegrationTests()
    {
        var directoryInfo =
            Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.Parent?.FullName ?? "";
        var envPath = Path.Combine(directoryInfo, ".env");
        Env.Load(envPath);

        var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");

        if (!string.IsNullOrWhiteSpace(token))
        {
            _isConfigured = true;
            _discordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = token,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
                LoggerFactory = NullLoggerFactory.Instance
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
        await Task.Delay(3000);
    }

    public async Task DisposeAsync()
    {
        if (_discordClient != null)
            await _discordClient.DisconnectAsync();
    }

    private DiscordChannel? GetTestChannel()
    {
        if (_discordClient == null) return null;
        if (!_discordClient.Guilds.TryGetValue(_guildId, out var guild)) return null;
        return guild.Channels.GetValueOrDefault(TestChannelId);
    }

    private DiscordChannel GetRequiredTestChannel()
    {
        var channel = GetTestChannel();
        if (channel is null)
            throw SkipException.ForSkip($"Integration test channel '{TestChannelId}' not found in guild '{_guildId}'.");
        return channel;
    }

    [Fact]
    public async Task Id_ReturnsCorrectChannelId()
    {
        if (!_isConfigured) return;
        await Task.CompletedTask;

        var channel = GetRequiredTestChannel();
        var wrapper = new DiscordChannelWrapper(channel);

        Assert.Equal(TestChannelId, wrapper.Id);
    }

    [Fact]
    public async Task Name_ReturnsNonEmptyChannelName()
    {
        if (!_isConfigured) return;
        await Task.CompletedTask;

        var channel = GetRequiredTestChannel();
        var wrapper = new DiscordChannelWrapper(channel);

        Assert.NotEmpty(wrapper.Name);
        Assert.Equal(channel.Name, wrapper.Name);
    }

    [Fact]
    public async Task ToDiscordChannel_ReturnsSameInstance()
    {
        if (!_isConfigured) return;
        await Task.CompletedTask;

        var channel = GetRequiredTestChannel();
        var wrapper = new DiscordChannelWrapper(channel);

        Assert.Equal(channel, wrapper.ToDiscordChannel());
    }

    [Fact]
    public async Task Guild_ReturnsDiscordGuildWrapper()
    {
        if (!_isConfigured) return;
        await Task.CompletedTask;

        var channel = GetRequiredTestChannel();
        var wrapper = new DiscordChannelWrapper(channel);

        Assert.IsType<DiscordGuildWrapper>(wrapper.Guild);
        Assert.IsAssignableFrom<IDiscordGuild>(wrapper.Guild);
    }

    [Fact]
    public async Task Guild_ReturnsCorrectGuildId()
    {
        if (!_isConfigured) return;
        await Task.CompletedTask;

        var channel = GetRequiredTestChannel();
        var wrapper = new DiscordChannelWrapper(channel);

        Assert.Equal(_guildId, wrapper.Guild.Id);
    }

    [Fact]
    public async Task SendMessageAsync_String_DoesNotThrow()
    {
        if (!_isConfigured) return;

        var channel = GetRequiredTestChannel();
        var wrapper = new DiscordChannelWrapper(channel);

        var ex = await Record.ExceptionAsync(() =>
            wrapper.SendMessageAsync($"Integration test message - string - {Guid.NewGuid():N}"));

        Assert.Null(ex);
    }

    [Fact]
    public async Task SendMessageAsync_Embed_DoesNotThrow()
    {
        if (!_isConfigured) return;

        var channel = GetRequiredTestChannel();
        var embed = new DiscordEmbedBuilder()
            .WithTitle("Integration Test")
            .WithDescription($"DiscordChannelWrapper integration test - embed - {Guid.NewGuid():N}")
            .Build();

        var wrapper = new DiscordChannelWrapper(channel);

        var ex = await Record.ExceptionAsync(() => wrapper.SendMessageAsync(embed));

        Assert.Null(ex);
    }
}
