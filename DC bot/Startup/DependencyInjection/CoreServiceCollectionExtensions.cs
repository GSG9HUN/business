using DC_bot.Configuration;
using DC_bot.Helper.Factory;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.IO;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Interface.Service.Presentation;
using DC_bot.IO;
using DC_bot.Service;
using DC_bot.Service.Core;
using DC_bot.Service.Presentation;
using DC_bot.Service.ReactionHandler;
using DC_bot.Wrapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DC_bot.Startup.DependencyInjection;

public static class CoreServiceCollectionExtensions
{
    
    public static IServiceCollection AddCoreBotServices(this IServiceCollection services, BotSettings botSettings)
    {
        return services
            .AddSingleton(botSettings)
            .AddSingleton<Func<IEnumerable<ICommand>>>(provider => () => provider.GetServices<ICommand>())
            .AddSingleton<ICommandRegistry, CommandRegistry>()
            .AddSingleton<IFileSystem, PhysicalFileSystem>()
            .AddSingleton<IDiscordMessageFactory, DiscordMessageWrapperFactory>()
            .AddSingleton<DiscordClientEventHandler>()
            .AddSingleton<BotService>()
            .AddSingleton<ReactionActionDispatcher>()
            .AddSingleton<ReactionContextFactory>()
            .AddSingleton(provider => new ReactionControlMessageService(
                provider.GetRequiredService<IProgressiveTimerService>(),
                provider.GetRequiredService<ILocalizationService>(),
                provider.GetRequiredService<ILogger<ReactionControlMessageService>>()))
            .AddSingleton<ReactionHandlerService>()
            .AddSingleton<CommandHandlerService>()
            .AddSingleton<IResponseBuilder, ResponseBuilder>()
            .AddSingleton<ICommandHelper, CommandValidationService>()
            .AddSingleton<IValidationService, ValidationService>()
            .AddSingleton<IUserValidationService, ValidationService>()
            .AddSingleton<ILocalizationService, LocalizationService>();
    }
}
