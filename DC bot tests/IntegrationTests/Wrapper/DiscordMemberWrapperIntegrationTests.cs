using DC_bot.Wrapper;
using DotNetEnv;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging.Abstractions;

namespace DC_bot_tests.IntegrationTests.Wrapper;

[Collection("Integration Tests")]
public class DiscordMemberWrapperIntegrationTests : IAsyncLifetime
{
    private readonly DiscordClient? _discordClient;
    private readonly bool _isConfigured;
    private readonly ulong _guildId = 1309813939563003966;

    public DiscordMemberWrapperIntegrationTests()
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

    private async Task<DiscordMember?> GetFirstMemberAsync()
    {
        if (_discordClient == null || !_isConfigured) return null;

        if (!_discordClient.Guilds.TryGetValue(_guildId, out var guild))
            return null;

        var members = await guild.GetAllMembersAsync();
        return members.FirstOrDefault();
    }

    [Fact]
    public async Task IsBot_ReturnsCorrectValue()
    {
        if (!_isConfigured)
        {
            return;
        }

        var discordMember = await GetFirstMemberAsync();
        Assert.NotNull(discordMember);

        var wrapper = new DiscordMemberWrapper(discordMember);
        
        Assert.Equal(discordMember.IsBot, wrapper.IsBot);
    }

    [Fact]
    public async Task Username_ReturnsCorrectValue()
    {
        if (!_isConfigured) return;

        var discordMember = await GetFirstMemberAsync();
        Assert.NotNull(discordMember);

        var wrapper = new DiscordMemberWrapper(discordMember);

        Assert.Equal(discordMember.Username, wrapper.Username);
        Assert.NotEmpty(wrapper.Username);
    }

    [Fact]
    public async Task Mention_ReturnsCorrectValue()
    {
        if (!_isConfigured) return;

        var discordMember = await GetFirstMemberAsync();
        Assert.NotNull(discordMember);

        var wrapper = new DiscordMemberWrapper(discordMember);

        Assert.Equal(discordMember.Mention, wrapper.Mention);
        Assert.StartsWith("<@", wrapper.Mention);
    }

    [Fact]
    public async Task VoiceState_ReturnsDiscordVoiceStateWrapper()
    {
        if (!_isConfigured) return;

        var discordMember = await GetFirstMemberAsync();
        Assert.NotNull(discordMember);

        var wrapper = new DiscordMemberWrapper(discordMember);
        
        Assert.IsType<DiscordVoiceStateWrapper>(wrapper.VoiceState);
    }

    [Fact]
    public async Task VoiceState_WhenMemberNotInVoiceChannel_ChannelIsNull()
    {
        if (!_isConfigured) return;

        var discordMember = await GetFirstMemberAsync();
        Assert.NotNull(discordMember);
        
        var wrapper = new DiscordMemberWrapper(discordMember); 
        Assert.Null(wrapper.VoiceState.Channel);
    }
}

