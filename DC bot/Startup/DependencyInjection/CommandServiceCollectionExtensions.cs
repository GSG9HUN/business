using DC_bot.Commands.SlashCommands.Music;
using DC_bot.Commands.SlashCommands.Queue;
using DC_bot.Commands.SlashCommands.Utility;
using DC_bot.Commands.TextCommands.Music;
using DC_bot.Commands.TextCommands.Queue;
using DC_bot.Commands.TextCommands.Utility;
using DC_bot.Interface;
using DC_bot.Interface.Service.SlashCommands;
using DC_bot.Service.SlashCommands;
using DC_bot.Wrapper;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot.Startup.DependencyInjection;

public static class CommandServiceCollectionExtensions
{
    public static IServiceCollection AddCommandServices(this IServiceCollection services)
    {
        return services
            .AddTextCommands()
            .AddSlashCommandServices()
            .AddSlashCommandProcessor();
    }

    private static IServiceCollection AddTextCommands(this IServiceCollection services)
    {
        return services
            .AddSingleton<ICommand, TagCommand>()
            .AddSingleton<ICommand, JoinCommand>()
            .AddSingleton<ICommand, PingCommand>()
            .AddSingleton<ICommand, HelpCommand>()
            .AddSingleton<ICommand, PlayCommand>()
            .AddSingleton<ICommand, SkipCommand>()
            .AddSingleton<ICommand, ClearCommand>()
            .AddSingleton<ICommand, LeaveCommand>()
            .AddSingleton<ICommand, PauseCommand>()
            .AddSingleton<ICommand, ResumeCommand>()
            .AddSingleton<ICommand, RepeatCommand>()
            .AddSingleton<ICommand, ShuffleCommand>()
            .AddSingleton<ICommand, LanguageCommand>()
            .AddSingleton<ICommand, ViewQueueCommand>()
            .AddSingleton<ICommand, RepeatListCommand>();
    }

    private static IServiceCollection AddSlashCommandServices(this IServiceCollection services)
    {
        return services
            .AddSingleton<ISlashCommandExecutor, SlashCommandExecutor>()
            .AddSingleton<ISlashInteractionContextFactory, SlashInteractionContextFactory>()
            .AddTransient<PingSlashCommand>()
            .AddTransient<HelpSlashCommand>()
            .AddTransient<TagSlashCommand>()
            .AddTransient<JoinSlashCommand>()
            .AddTransient<PlaySlashCommand>()
            .AddTransient<SkipSlashCommand>()
            .AddTransient<PauseSlashCommand>()
            .AddTransient<ResumeSlashCommand>()
            .AddTransient<LeaveSlashCommand>()
            .AddTransient<QueueSlashCommand>()
            .AddTransient<ShuffleSlashCommand>()
            .AddTransient<RepeatSlashCommand>()
            .AddTransient<LanguageSlashCommand>()
            .AddTransient<ClearSlashCommand>();
    }


    private static IServiceCollection AddSlashCommandProcessor(this IServiceCollection services)
    {
        return services.AddCommandsExtension((_, extension) =>
        {
            extension.AddCommands(
            [
                typeof(PingSlashCommand),
                typeof(HelpSlashCommand),
                typeof(TagSlashCommand),
                typeof(JoinSlashCommand),
                typeof(PlaySlashCommand),
                typeof(SkipSlashCommand),
                typeof(PauseSlashCommand),
                typeof(ResumeSlashCommand),
                typeof(LeaveSlashCommand),
                typeof(QueueSlashCommand),
                typeof(ShuffleSlashCommand),
                typeof(RepeatSlashCommand),
                typeof(LanguageSlashCommand),
                typeof(ClearSlashCommand)
            ]);
            extension.AddProcessor(new SlashCommandProcessor());
        }, new CommandsConfiguration
        {
            RegisterDefaultCommandProcessors = false
        });
    }

    
}