using DC_bot.Interface.Discord;
using DC_bot.Wrapper;
using DSharpPlus;
using DSharpPlus.Entities;

namespace DC_bot_tests.EndToEndTests.Wrapper;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class DiscordGuildWrapperEndToEndTests : IAsyncLifetime
{
    private readonly DiscordClient? _discordClient;
    private readonly bool _isConfigured;
    private readonly ulong _guildId;
    private bool _isDiscordAvailable;

    public DiscordGuildWrapperEndToEndTests()
    {
        var hasToken = EndToEndTestConfiguration.TryGetDiscordToken(out var token);
        var hasGuild = EndToEndTestConfiguration.TryGetDiscordGuildId(out var guildId);

        _guildId = guildId;

        if (hasToken && hasGuild)
        {
            _isConfigured = true;
            _discordClient = TestDiscordClientFactory.Create(
                token,
                DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers);
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

    private async Task<DiscordGuild?> GetGuildAsync()
    {
        if (!CanRun()) return null;

        try
        {
            return await _discordClient!.GetGuildAsync(_guildId);
        }
        catch (Exception exception) when (EndToEndDiscordGuard.IsDiscordEnvironmentUnavailable(exception))
        {
            return null;
        }
    }

    [Fact]
    public async Task Id_ReturnsCorrectGuildId()
    {
        var guild = await GetGuildAsync();
        if (guild is null) return;

        var wrapper = new DiscordGuildWrapper(guild);

        Assert.Equal(_guildId, wrapper.Id);
    }

    [Fact]
    public async Task Name_ReturnsNonEmptyGuildName()
    {
        var guild = await GetGuildAsync();
        if (guild is null) return;

        var wrapper = new DiscordGuildWrapper(guild);

        Assert.NotEmpty(wrapper.Name);
        Assert.Equal(guild.Name, wrapper.Name);
    }

    [Fact]
    public async Task ToDiscordGuild_ReturnsSameInstance()
    {
        var guild = await GetGuildAsync();
        if (guild is null) return;

        var wrapper = new DiscordGuildWrapper(guild);

        Assert.Equal(guild, wrapper.ToDiscordGuild());
    }

    [Fact]
    public async Task GetAllMembersAsync_ReturnsNonEmptyCollection()
    {
        var guild = await GetGuildAsync();
        if (guild is null) return;

        var wrapper = new DiscordGuildWrapper(guild);
        var members = await wrapper.GetAllMembersAsync();

        Assert.NotEmpty(members);
        Assert.All(members, m => Assert.IsType<DiscordMemberWrapper>(m));
    }

    [Fact]
    public async Task GetAllMembersAsync_AllMembersImplementIDiscordMember()
    {
        var guild = await GetGuildAsync();
        if (guild is null) return;

        var wrapper = new DiscordGuildWrapper(guild);
        var members = await wrapper.GetAllMembersAsync();

        Assert.All(members, m => Assert.IsAssignableFrom<IDiscordMember>(m));
    }

    [Fact]
    public async Task GetMemberAsync_WithValidId_ReturnsCorrectMember()
    {
        var guild = await GetGuildAsync();
        if (guild is null) return;

        var allMembers = await guild.GetAllMembersAsync();
        var firstMember = allMembers.FirstOrDefault();
        if (firstMember is null) return;

        var wrapper = new DiscordGuildWrapper(guild);
        var fetchedMember = await wrapper.GetMemberAsync(firstMember.Id);

        Assert.IsType<DiscordMemberWrapper>(fetchedMember);
        Assert.Equal(firstMember.Id, fetchedMember.Id);
    }

    private bool CanRun()
    {
        return _isConfigured && _isDiscordAvailable && _discordClient != null;
    }
}