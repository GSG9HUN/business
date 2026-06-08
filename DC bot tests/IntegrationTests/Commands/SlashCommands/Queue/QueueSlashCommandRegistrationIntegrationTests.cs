using DC_bot.Commands.SlashCommands.Queue;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot_tests.IntegrationTests.Commands.SlashCommands.Queue;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class QueueSlashCommandRegistrationIntegrationTests : SlashCommandRegistrationIntegrationTestBase
{
    [Fact]
    public async Task Create_ShouldRegisterQueueSlashCommandModules()
    {
        await WithServiceProviderAsync(services =>
        {
            Assert.NotNull(services.GetRequiredService<QueueSlashCommand>());
            Assert.NotNull(services.GetRequiredService<ShuffleSlashCommand>());
            Assert.NotNull(services.GetRequiredService<RepeatSlashCommand>());
            Assert.NotNull(services.GetRequiredService<ClearSlashCommand>());
        });
    }
}
