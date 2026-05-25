using System.Reflection;
using System.Runtime.CompilerServices;
using DC_bot.Configuration;
using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Service.Core;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.IntegrationTests.Service.Core;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class CommandHandlerServiceIntegrationTests
{
    [Fact]
    public async Task HandleCommandAsync_WithFakeDiscordMessageEvent_ExecutesRegisteredCommand()
    {
        var command = new Mock<ICommand>();
        var executed = new TaskCompletionSource<IDiscordMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        command.SetupGet(x => x.Name).Returns("fake");
        command.SetupGet(x => x.Description).Returns("Fake command");
        command.Setup(x => x.ExecuteAsync(It.IsAny<IDiscordMessage>()))
            .Callback<IDiscordMessage>(message => executed.SetResult(message))
            .Returns(Task.CompletedTask);

        await using var services = new ServiceCollection()
            .AddSingleton<ICommand>(command.Object)
            .BuildServiceProvider();

        var logger = new Mock<ILogger<CommandHandlerService>>();
        logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        var service = new CommandHandlerService(
            services,
            logger.Object,
            Mock.Of<ILocalizationService>(),
            new BotSettings { Prefix = "!" },
            isTestMode: true);
        var args = CreateMessageCreateEventArgs("!fake with args", isBot: false);

        await InvokeHandleCommandAsync(service, args);

        var handledMessage = await executed.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Equal("!fake with args", handledMessage.Content);
        command.Verify(x => x.ExecuteAsync(It.IsAny<IDiscordMessage>()), Times.Once);
    }

    [Fact]
    public async Task HandleCommandAsync_WithFakeDiscordMessageWithoutPrefix_DoesNotExecuteCommand()
    {
        var command = new Mock<ICommand>();
        command.SetupGet(x => x.Name).Returns("fake");
        command.SetupGet(x => x.Description).Returns("Fake command");

        await using var services = new ServiceCollection()
            .AddSingleton<ICommand>(command.Object)
            .BuildServiceProvider();

        var service = new CommandHandlerService(
            services,
            Mock.Of<ILogger<CommandHandlerService>>(),
            Mock.Of<ILocalizationService>(),
            new BotSettings { Prefix = "!" },
            isTestMode: true);
        var args = CreateMessageCreateEventArgs("fake without prefix", isBot: false);

        await InvokeHandleCommandAsync(service, args);

        command.Verify(x => x.ExecuteAsync(It.IsAny<IDiscordMessage>()), Times.Never);
    }

    [Fact]
    public async Task HandleCommandAsync_WithBotAuthorAndProductionMode_DoesNotExecuteCommand()
    {
        var command = new Mock<ICommand>();
        command.SetupGet(x => x.Name).Returns("fake");
        command.SetupGet(x => x.Description).Returns("Fake command");

        await using var services = new ServiceCollection()
            .AddSingleton<ICommand>(command.Object)
            .BuildServiceProvider();

        var service = new CommandHandlerService(
            services,
            Mock.Of<ILogger<CommandHandlerService>>(),
            Mock.Of<ILocalizationService>(),
            new BotSettings { Prefix = "!" });
        var args = CreateMessageCreateEventArgs("!fake", isBot: true);

        await InvokeHandleCommandAsync(service, args);

        command.Verify(x => x.ExecuteAsync(It.IsAny<IDiscordMessage>()), Times.Never);
    }

    private static async Task InvokeHandleCommandAsync(CommandHandlerService service, MessageCreateEventArgs args)
    {
        var method = typeof(CommandHandlerService).GetMethod("HandleCommandAsync",
            BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.NotNull(method);

        using var client = new DiscordClient(new DiscordConfiguration
        {
            Token = "fake-token",
            Intents = DiscordIntents.AllUnprivileged
        });

        var task = (Task?)method.Invoke(service, [client, args]);
        Assert.NotNull(task);
        await task!;
    }

    private static MessageCreateEventArgs CreateMessageCreateEventArgs(string content, bool isBot)
    {
        var author = Create<DiscordUser>();
        SetMember(author, "Id", 123ul);
        SetMember(author, "Username", "IntegrationUser");
        SetMember(author, "IsBot", isBot);

        var guild = Create<DiscordGuild>();
        SetMember(guild, "Id", 456ul);
        SetMember(guild, "Name", "IntegrationGuild");

        var channel = Create<DiscordChannel>();
        SetMember(channel, "Id", 789ul);
        SetMember(channel, "Name", "integration-channel");
        TrySetMember(channel, "Guild", guild);

        var message = Create<DiscordMessage>();
        SetMember(message, "Id", 999ul);
        SetMember(message, "Content", content);
        SetMember(message, "Embeds", new List<DiscordEmbed>());
        TrySetMember(message, "Author", author);
        TrySetMember(message, "Channel", channel);

        var args = Create<MessageCreateEventArgs>();
        SetMember(args, "Message", message);
        TrySetMember(args, "Author", author);
        TrySetMember(args, "Channel", channel);
        TrySetMember(args, "Guild", guild);

        return args;
    }

    private static T Create<T>() => (T)RuntimeHelpers.GetUninitializedObject(typeof(T));

    private static bool TrySetMember(object obj, string name, object? value)
    {
        var type = obj.GetType();
        while (type is not null)
        {
            var backingField = type.GetField($"<{name}>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (backingField is not null)
            {
                backingField.SetValue(obj, value);
                return true;
            }

            var underscored = type.GetField($"_{char.ToLowerInvariant(name[0])}{name[1..]}",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (underscored is not null)
            {
                underscored.SetValue(obj, value);
                return true;
            }

            var property = type.GetProperty(name,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (property?.SetMethod is not null)
            {
                property.SetValue(obj, value);
                return true;
            }

            type = type.BaseType;
        }

        return false;
    }

    private static void SetMember(object obj, string name, object? value)
    {
        if (!TrySetMember(obj, name, value))
        {
            throw new InvalidOperationException($"Member '{name}' not found on {obj.GetType().Name}.");
        }
    }
}
