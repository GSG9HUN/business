using DC_bot.Commands.SlashCommands.Utility;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot_tests.IntegrationTests.Commands.SlashCommands.Utility;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class TagSlashCommandRegistrationIntegrationTests : SlashCommandRegistrationIntegrationTestBase
{
    [Fact]
    public async Task Create_ShouldResolveTagSlashCommandWithMemberOption()
    {
        await WithServiceProviderAsync(services =>
        {
            var command = services.GetRequiredService<TagSlashCommand>();
            var userParameter = command.GetType()
                .GetMethod(nameof(TagSlashCommand.Tag))!
                .GetParameters()[1];

            Assert.Equal(typeof(DiscordMember), userParameter.ParameterType);
        });
    }
}
