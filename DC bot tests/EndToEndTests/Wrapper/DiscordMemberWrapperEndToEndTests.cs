using DC_bot.Wrapper;
using DSharpPlus;
using DSharpPlus.Entities;

namespace DC_bot_tests.EndToEndTests.Wrapper;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class DiscordMemberWrapperEndToEndTests : IAsyncLifetime
{
    private readonly DiscordClient? _discordClient;
    private readonly bool _isConfigured;
    private readonly ulong _guildId;
    private bool _isDiscordAvailable;

    public DiscordMemberWrapperEndToEndTests()
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

    private async Task<DiscordMember?> GetFirstMemberAsync()
    {
        if (!CanRun()) return null;

        try
        {
            var guild = await _discordClient!.GetGuildAsync(_guildId);
            var members = await guild.GetAllMembersAsync();
            return members.FirstOrDefault();
        }
        catch (Exception exception) when (EndToEndDiscordGuard.IsDiscordEnvironmentUnavailable(exception))
        {
            return null;
        }
    }

    [Fact]
    public async Task IsBot_ReturnsCorrectValue()
    {
        var discordMember = await GetFirstMemberAsync();
        if (discordMember is null) return;

        var wrapper = new DiscordMemberWrapper(discordMember);

        Assert.Equal(discordMember.IsBot, wrapper.IsBot);
    }

    [Fact]
    public async Task Username_ReturnsCorrectValue()
    {
        var discordMember = await GetFirstMemberAsync();
        if (discordMember is null) return;

        var wrapper = new DiscordMemberWrapper(discordMember);

        Assert.Equal(discordMember.Username, wrapper.Username);
        Assert.NotEmpty(wrapper.Username);
    }

    [Fact]
    public async Task Mention_ReturnsCorrectValue()
    {
        var discordMember = await GetFirstMemberAsync();
        if (discordMember is null) return;

        var wrapper = new DiscordMemberWrapper(discordMember);

        Assert.Equal(discordMember.Mention, wrapper.Mention);
        Assert.StartsWith("<@", wrapper.Mention);
    }

    [Fact]
    public async Task VoiceState_ReturnsDiscordVoiceStateWrapper()
    {
        var discordMember = await GetFirstMemberAsync();
        if (discordMember is null) return;

        var wrapper = new DiscordMemberWrapper(discordMember);

        Assert.IsType<DiscordVoiceStateWrapper>(wrapper.VoiceState);
    }

    [Fact]
    public async Task VoiceState_WhenMemberNotInVoiceChannel_ChannelIsNull()
    {
        var discordMember = await GetFirstMemberAsync();
        if (discordMember is null) return;

        var wrapper = new DiscordMemberWrapper(discordMember);
        Assert.Null(wrapper.VoiceState.Channel);
    }

    private bool CanRun()
    {
        return _isConfigured && _isDiscordAvailable && _discordClient != null;
    }
}