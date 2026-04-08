using DC_bot.Interface.Discord;
using DC_bot.Wrapper;
using DotNetEnv;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging.Abstractions;

namespace DC_bot_tests.IntegrationTests.Wrapper;

[Collection("Integration Tests")]
public class DiscordGuildWrapperIntegrationTests : IAsyncLifetime
{
    private readonly DiscordClient? _discordClient;
    private readonly bool _isConfigured;
    private readonly ulong _guildId = 1309813939563003966;

    public DiscordGuildWrapperIntegrationTests()
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
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers,
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

    private DiscordGuild? GetGuild() =>
        _discordClient?.Guilds.GetValueOrDefault(_guildId);

    [Fact]
    public async Task Id_ReturnsCorrectGuildId()
    {
        if (!_isConfigured) return;
        await Task.CompletedTask;

        var guild = GetGuild();
        Assert.NotNull(guild);

        var wrapper = new DiscordGuildWrapper(guild);

        Assert.Equal(_guildId, wrapper.Id);
    }

    [Fact]
    public async Task Name_ReturnsNonEmptyGuildName()
    {
        if (!_isConfigured) return;
        await Task.CompletedTask;

        var guild = GetGuild();
        Assert.NotNull(guild);

        var wrapper = new DiscordGuildWrapper(guild);

        Assert.NotEmpty(wrapper.Name);
        Assert.Equal(guild.Name, wrapper.Name);
    }

    [Fact]
    public async Task ToDiscordGuild_ReturnsSameInstance()
    {
        if (!_isConfigured) return;
        await Task.CompletedTask;

        var guild = GetGuild();
        Assert.NotNull(guild);

        var wrapper = new DiscordGuildWrapper(guild);

        Assert.Equal(guild, wrapper.ToDiscordGuild());
    }

    [Fact]
    public async Task GetAllMembersAsync_ReturnsNonEmptyCollection()
    {
        if (!_isConfigured) return;

        var guild = GetGuild();
        Assert.NotNull(guild);

        var wrapper = new DiscordGuildWrapper(guild);
        var members = await wrapper.GetAllMembersAsync();

        Assert.NotEmpty(members);
        Assert.All(members, m => Assert.IsType<DiscordMemberWrapper>(m));
    }

    [Fact]
    public async Task GetAllMembersAsync_AllMembersImplementIDiscordMember()
    {
        if (!_isConfigured) return;

        var guild = GetGuild();
        Assert.NotNull(guild);

        var wrapper = new DiscordGuildWrapper(guild);
        var members = await wrapper.GetAllMembersAsync();

        Assert.All(members, m => Assert.IsAssignableFrom<IDiscordMember>(m));
    }

    [Fact]
    public async Task GetMemberAsync_WithValidId_ReturnsCorrectMember()
    {
        if (!_isConfigured) return;

        var guild = GetGuild();
        Assert.NotNull(guild);

        var allMembers = await guild.GetAllMembersAsync();
        var firstMember = allMembers.First();

        var wrapper = new DiscordGuildWrapper(guild);
        var fetchedMember = await wrapper.GetMemberAsync(firstMember.Id);

        Assert.IsType<DiscordMemberWrapper>(fetchedMember);
        Assert.Equal(firstMember.Id, fetchedMember.Id);
    }
}

