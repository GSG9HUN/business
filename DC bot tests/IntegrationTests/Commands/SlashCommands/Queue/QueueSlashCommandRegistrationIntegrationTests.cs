using DC_bot.Commands.SlashCommands.Queue;

namespace DC_bot_tests.IntegrationTests.Commands.SlashCommands.Queue;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class QueueSlashCommandRegistrationIntegrationTests : SlashCommandRegistrationIntegrationTestBase
{
    [Fact]
    public async Task Create_ShouldRegisterQueueSlashCommandModules()
    {
        await WithServiceProviderAsync(services => services.AssertResolvesRequiredServices(
            typeof(QueueSlashCommand),
            typeof(ShuffleSlashCommand),
            typeof(RepeatSlashCommand),
            typeof(ClearSlashCommand)));
    }
}
