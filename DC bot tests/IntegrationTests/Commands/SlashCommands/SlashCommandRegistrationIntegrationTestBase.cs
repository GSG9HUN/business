using DC_bot.Configuration;
using DC_bot.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot_tests.IntegrationTests.Commands.SlashCommands;

public abstract class SlashCommandRegistrationIntegrationTestBase
{
    protected static Task WithServiceProviderAsync(Action<ServiceProvider> test)
    {
        return WithServiceProviderAsync(provider =>
        {
            test(provider);
            return Task.CompletedTask;
        });
    }

    protected static async Task WithServiceProviderAsync(Func<ServiceProvider, Task> test)
    {
        var provider = CreateServiceProvider();
        try
        {
            await test(provider);
        }
        finally
        {
            await ServiceProviderDisposeHelper.DisposeIgnoringDisconnectedDiscordClientAsync(provider);
        }
    }

    protected static ServiceProvider CreateServiceProvider()
    {
        return BotServiceProviderFactory.Create(
            new BotSettings { Token = "fake-token", Prefix = "!" },
            new LavalinkSettings
            {
                Hostname = "localhost",
                Port = 2333,
                Password = "password",
                Secured = false
            },
            "Host=localhost;Port=5432;Database=bot_tests;Username=postgres;Password=postgres");
    }
}
