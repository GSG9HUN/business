using System.Reflection;
using System.Runtime.CompilerServices;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DC_bot_tests.IntegrationTests.Service.Core;

internal static class FakeDiscordMessageBuilder
{
    public static MessageCreatedEventArgs CreateMessageCreateEventArgs(string content, bool isBot)
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
        SetMember(message, "embeds", new List<DiscordEmbed>());
        TrySetMember(message, "Author", author);
        TrySetMember(message, "Channel", channel);

        var args = Create<MessageCreatedEventArgs>();
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

            var matchingField = type
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .FirstOrDefault(field =>
                    string.Equals(field.Name, name, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(field.Name.TrimStart('_'), name, StringComparison.OrdinalIgnoreCase));
            if (matchingField is not null)
            {
                matchingField.SetValue(obj, value);
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
