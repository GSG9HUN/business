using DC_bot.Interface.Discord;
using DC_bot.Wrapper;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Sdk;

namespace DC_bot_tests.EndToEndTests.Wrapper;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class DiscordGuildWrapperEndToEndTests : IAsyncLifetime
{
    private readonly DiscordClient? _discordClient;
    private readonly bool _isConfigured;
    private readonly ulong _guildId;

    public DiscordGuildWrapperEndToEndTests()
    {
        var hasToken = EndToEndTestConfiguration.TryGetDiscordToken(out var token);
        var hasGuild = EndToEndTestConfiguration.TryGetDiscordGuildId(out var guildId);

        _guildId = guildId;

        if (hasToken && hasGuild)
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
        {
            await _discordClient.DisconnectAsync();
        }
    }

    private DiscordGuild? GetGuild() =>
        _discordClient?.Guilds.GetValueOrDefault(_guildId);

    [Fact]
    public void Id_ReturnsCorrectGuildId()
    {
        EnsureConfigured();

        var guild = GetGuild();
        Assert.NotNull(guild);

        var wrapper = new DiscordGuildWrapper(guild);

        Assert.Equal(_guildId, wrapper.Id);
    }

    [Fact]
    public void Name_ReturnsNonEmptyGuildName()
    {
        EnsureConfigured();

        var guild = GetGuild();
        Assert.NotNull(guild);

        var wrapper = new DiscordGuildWrapper(guild);

        Assert.NotEmpty(wrapper.Name);
        Assert.Equal(guild.Name, wrapper.Name);
    }

    [Fact]
    public void ToDiscordGuild_ReturnsSameInstance()
    {
        EnsureConfigured();

        var guild = GetGuild();
        Assert.NotNull(guild);

        var wrapper = new DiscordGuildWrapper(guild);

        Assert.Equal(guild, wrapper.ToDiscordGuild());
    }

    [Fact]
    public async Task GetAllMembersAsync_ReturnsNonEmptyCollection()
    {
        EnsureConfigured();

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
        EnsureConfigured();

        var guild = GetGuild();
        Assert.NotNull(guild);

        var wrapper = new DiscordGuildWrapper(guild);
        var members = await wrapper.GetAllMembersAsync();

        Assert.All(members, m => Assert.IsAssignableFrom<IDiscordMember>(m));
    }

    [Fact]
    public async Task GetMemberAsync_WithValidId_ReturnsCorrectMember()
    {
        EnsureConfigured();

        var guild = GetGuild();
        Assert.NotNull(guild);

        var allMembers = await guild.GetAllMembersAsync();
        var firstMember = allMembers.First();

        var wrapper = new DiscordGuildWrapper(guild);
        var fetchedMember = await wrapper.GetMemberAsync(firstMember.Id);

        Assert.IsType<DiscordMemberWrapper>(fetchedMember);
        Assert.Equal(firstMember.Id, fetchedMember.Id);
    }

    private void EnsureConfigured()
    {
        if (!_isConfigured || _discordClient == null)
        {
            throw SkipException.ForSkip(EndToEndTestConfiguration.MissingDiscordTokenAndGuildMessage());
        }
    }
}

