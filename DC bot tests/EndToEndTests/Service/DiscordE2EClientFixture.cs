using DC_bot.Configuration;
using DC_bot.Interface.Service.Music;
using DC_bot.Service;
using DC_bot.Service.ReactionHandler;
using DC_bot.Startup;
using DC_bot_tests.IntegrationTests.Persistence;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace DC_bot_tests.EndToEndTests.Service;

internal sealed class DiscordE2EClientFixture : IAsyncDisposable
{
    private readonly PostgreSqlTestDatabase _database;
    private readonly ReactionHandlerService _reactionHandlerService;

    private DiscordE2EClientFixture(
        PostgreSqlTestDatabase database,
        ServiceProvider provider,
        DiscordClient client,
        ReactionHandlerService reactionHandlerService,
        DiscordGuild guild,
        DiscordChannel textChannel,
        DiscordChannel voiceChannel,
        DiscordMessage testRunMarker)
    {
        _database = database;
        Provider = provider;
        Client = client;
        _reactionHandlerService = reactionHandlerService;
        Guild = guild;
        TextChannel = textChannel;
        VoiceChannel = voiceChannel;
        TestRunMarker = testRunMarker;
    }

    public ServiceProvider Provider { get; }
    public DiscordClient Client { get; }
    public DiscordGuild Guild { get; }
    public ulong GuildId => Guild.Id;
    public DiscordChannel TextChannel { get; }
    public DiscordChannel VoiceChannel { get; }
    public DiscordMessage TestRunMarker { get; }

    public static async Task<DiscordE2EClientFixture?> TryCreateAsync(ITestOutputHelper testOutputHelper)
    {
        var hasToken = EndToEndTestConfiguration.TryGetDiscordToken(out var token);
        var hasGuild = EndToEndTestConfiguration.TryGetDiscordGuildId(out var guildId);
        var hasTextChannel = EndToEndTestConfiguration.TryGetDiscordChannelId(out var textChannelId);
        var hasVoiceChannel = EndToEndTestConfiguration.TryGetDiscordVoiceChannelId(out var voiceChannelId);
        var hasLavalink = EndToEndTestConfiguration.TryGetLavalinkSettings(out var lavalinkSettings);
        if (!hasToken || !hasGuild || !hasTextChannel || !hasVoiceChannel || !hasLavalink)
        {
            testOutputHelper.WriteLine(EndToEndTestConfiguration.MissingMusicFlowMessage());
            return null;
        }

        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null)
        {
            testOutputHelper.WriteLine("E2E music flow test requires Docker for PostgreSQL.");
            return null;
        }

        var provider = BotServiceProviderFactory.Create(
            new BotSettings { Token = token, Prefix = "!" },
            lavalinkSettings,
            database.ConnectionString);

        try
        {
            await DatabaseMigrationRunner.ApplyMigrationsIfNeededAsync(provider);

            var client = provider.GetRequiredService<DiscordClient>();
            await provider.GetRequiredService<BotService>().StartAsync(isTestEnvironment: true);
            await provider.GetRequiredService<ILavaLinkService>().ConnectAsync();

            var reactionHandler = provider.GetRequiredService<ReactionHandlerService>();
            reactionHandler.RegisterHandler(client);

            var discordGuild = await client.GetGuildAsync(guildId);
            var discordTextChannel = await client.GetChannelAsync(textChannelId);
            var discordVoiceChannel = await client.GetChannelAsync(voiceChannelId);

            Assert.Equal(guildId, discordGuild.Id);
            Assert.Equal(textChannelId, discordTextChannel.Id);
            Assert.Equal(voiceChannelId, discordVoiceChannel.Id);
            testOutputHelper.WriteLine(
                $"Music flow target voice channel: Id={discordVoiceChannel.Id}, Name={discordVoiceChannel.Name}, Type={discordVoiceChannel.Type}");

            var testRunMarker = await discordTextChannel.SendMessageAsync(
                $"E2E music flow started: {Guid.NewGuid():N}");

            return new DiscordE2EClientFixture(
                database,
                provider,
                client,
                reactionHandler,
                discordGuild,
                discordTextChannel,
                discordVoiceChannel,
                testRunMarker);
        }
        catch
        {
            await ServiceProviderDisposeHelper.DisposeIgnoringDisconnectedDiscordClientAsync(provider);
            await database.DisposeAsync();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        _reactionHandlerService.UnregisterHandler(Client);
        await ServiceProviderDisposeHelper.DisposeIgnoringDisconnectedDiscordClientAsync(Provider);
        await _database.DisposeAsync();
    }
}
