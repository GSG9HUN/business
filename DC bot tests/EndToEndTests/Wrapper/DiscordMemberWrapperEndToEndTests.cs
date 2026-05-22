using DC_bot.Wrapper;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Sdk;

namespace DC_bot_tests.EndToEndTests.Wrapper;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class DiscordMemberWrapperEndToEndTests : IAsyncLifetime
{
    private readonly DiscordClient? _discordClient;
    private readonly bool _isConfigured;
    private readonly ulong _guildId;

    public DiscordMemberWrapperEndToEndTests()
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

    private async Task<DiscordMember> GetFirstMemberAsync()
    {
        EnsureConfigured();

        if (!_discordClient!.Guilds.TryGetValue(_guildId, out var guild))
        {
            throw SkipException.ForSkip($"E2E test guild '{_guildId}' was not available.");
        }

        var members = await guild.GetAllMembersAsync();
        return members.FirstOrDefault()
               ?? throw SkipException.ForSkip($"E2E test guild '{_guildId}' has no members.");
    }

    [Fact]
    public async Task IsBot_ReturnsCorrectValue()
    {
        var discordMember = await GetFirstMemberAsync();

        var wrapper = new DiscordMemberWrapper(discordMember);

        Assert.Equal(discordMember.IsBot, wrapper.IsBot);
    }

    [Fact]
    public async Task Username_ReturnsCorrectValue()
    {
        var discordMember = await GetFirstMemberAsync();

        var wrapper = new DiscordMemberWrapper(discordMember);

        Assert.Equal(discordMember.Username, wrapper.Username);
        Assert.NotEmpty(wrapper.Username);
    }

    [Fact]
    public async Task Mention_ReturnsCorrectValue()
    {
        var discordMember = await GetFirstMemberAsync();

        var wrapper = new DiscordMemberWrapper(discordMember);

        Assert.Equal(discordMember.Mention, wrapper.Mention);
        Assert.StartsWith("<@", wrapper.Mention);
    }

    [Fact]
    public async Task VoiceState_ReturnsDiscordVoiceStateWrapper()
    {
        var discordMember = await GetFirstMemberAsync();

        var wrapper = new DiscordMemberWrapper(discordMember);

        Assert.IsType<DiscordVoiceStateWrapper>(wrapper.VoiceState);
    }

    [Fact]
    public async Task VoiceState_WhenMemberNotInVoiceChannel_ChannelIsNull()
    {
        var discordMember = await GetFirstMemberAsync();

        var wrapper = new DiscordMemberWrapper(discordMember);
        Assert.Null(wrapper.VoiceState.Channel);
    }

    private void EnsureConfigured()
    {
        if (!_isConfigured || _discordClient == null)
        {
            throw SkipException.ForSkip(EndToEndTestConfiguration.MissingDiscordTokenAndGuildMessage());
        }
    }
}

