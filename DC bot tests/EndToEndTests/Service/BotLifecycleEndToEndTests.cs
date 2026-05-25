using DC_bot.Service;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Sdk;

namespace DC_bot_tests.EndToEndTests.Service;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class BotLifecycleEndToEndTests
{
    [Fact]
    public async Task BotStartupShutdown_WithRealDiscordTokenAndTestGuild_ConnectsResolvesGuildAndDisconnects()
    {
        var hasToken = EndToEndTestConfiguration.TryGetDiscordToken(out var token);
        var hasGuild = EndToEndTestConfiguration.TryGetDiscordGuildId(out var guildId);
        if (!hasToken || !hasGuild)
        {
            throw SkipException.ForSkip(EndToEndTestConfiguration.MissingDiscordTokenAndGuildMessage());
        }

        var logger = new Mock<ILogger<BotService>>();
        logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        using var client = new DiscordClient(new DiscordConfiguration
        {
            Token = token,
            Intents = DiscordIntents.AllUnprivileged
        });
        var service = new BotService(client, logger.Object);

        await service.StartAsync(isTestEnvironment: true);

        var guild = await client.GetGuildAsync(guildId);
        Assert.NotNull(guild);

        await client.DisconnectAsync();
    }
}
