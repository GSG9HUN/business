using System.Reflection;
using DC_bot.Configuration;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Service.Core;
using DC_bot.Service.ReactionHandler;
using DC_bot.Startup;
using DC_bot_tests.TestHelperFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Startup;

[Trait("Category", "Unit")]
public class BotHandlerRegistrarTests
{
    [Fact]
    public async Task RegisterHandlers_RegistersCommandAndReactionHandlers()
    {
        var commandRegistry = new Mock<ICommandRegistry>();
        commandRegistry.SetupGet(registry => registry.Commands).Returns(Array.Empty<ICommand>());

        var commandHandler = new CommandHandlerService(
            commandRegistry.Object,
            Mock.Of<ILogger<CommandHandlerService>>(),
            Mock.Of<ILocalizationService>(),
            new BotSettings { Prefix = "!" });
        var reactionHandler = new ReactionHandlerService(
            Mock.Of<ILavaLinkService>(),
            Mock.Of<ILogger<ReactionHandlerService>>(),
            Mock.Of<IProgressiveTimerService>(),
            Mock.Of<ILocalizationService>());

        var provider = new ServiceCollection()
            .AddSingleton(TestDiscordClientFactory.Create())
            .AddSingleton(commandHandler)
            .AddSingleton(reactionHandler)
            .BuildServiceProvider();

        try
        {
            BotHandlerRegistrar.RegisterHandlers(provider);

            Assert.True(GetIsRegistered(commandHandler));
            Assert.True(GetIsRegistered(reactionHandler));
        }
        finally
        {
            await ServiceProviderDisposeHelper.DisposeIgnoringDisconnectedDiscordClientAsync(provider);
        }
    }

    private static bool GetIsRegistered(object handler)
    {
        var field = handler.GetType().GetField("_isRegistered", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        return (bool)field.GetValue(handler)!;
    }
}
