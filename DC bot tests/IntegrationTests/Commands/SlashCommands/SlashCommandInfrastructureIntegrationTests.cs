using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot_tests.IntegrationTests.Commands.SlashCommands;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class SlashCommandInfrastructureIntegrationTests : SlashCommandRegistrationIntegrationTestBase
{
    [Fact]
    public async Task Create_ShouldRegisterSlashCommandServices()
    {
        await WithServiceProviderAsync(services =>
        {
            Assert.NotNull(services.GetRequiredService<ISlashCommandExecutor>());
            Assert.NotNull(services.GetRequiredService<ISlashInteractionContextFactory>());
        });
    }

    [Fact]
    public async Task Create_ShouldRegisterDSharpPlusCommandsExtensionWithSlashProcessor()
    {
        await WithServiceProviderAsync(services =>
        {
            var extension = services.GetRequiredService<CommandsExtension>();

            Assert.NotNull(extension);
            Assert.True(extension.TryGetProcessor<SlashCommandProcessor>(out var slashProcessor));
            Assert.NotNull(slashProcessor);
        });
    }

    [Fact]
    public async Task ExecutorFromStartupGraph_ShouldExecutePingSlashPipeline()
    {
        await WithServiceProviderAsync(async services =>
        {
            var executor = services.GetRequiredService<ISlashCommandExecutor>();
            var context = new TestSlashInteractionContext();

            await executor.ExecuteAsync(new SlashCommandExecutionRequest("ping", context));

            Assert.Contains("Pong!", context.TextResponses);
        });
    }
}
