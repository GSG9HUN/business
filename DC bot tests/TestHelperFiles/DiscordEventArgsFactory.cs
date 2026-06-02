using System.Reflection;
using System.Runtime.CompilerServices;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DC_bot_tests.TestHelperFiles;

public static class DiscordEventArgsFactory
{
    public static DiscordGuild CreateGuild(ulong id, string name = "E2E Test Guild")
    {
        var guild = Create<DiscordGuild>();
        SetMember(guild, "Id", id);
        SetMember(guild, "Name", name);

        return guild;
    }

    public static MessageCreatedEventArgs CreateMessageCreated(DiscordMessage message, DiscordGuild? guild = null)
    {
        var args = Create<MessageCreatedEventArgs>();
        var resolvedGuild = guild ?? message.Channel?.Guild;
        if (message.Channel is not null && resolvedGuild is not null)
        {
            TrySetMember(message.Channel, "Guild", resolvedGuild);
        }

        SetMember(args, "Message", message);
        TrySetMember(args, "Author", message.Author);
        TrySetMember(args, "Channel", message.Channel);
        TrySetMember(args, "Guild", resolvedGuild);

        return args;
    }

    public static MessageReactionAddedEventArgs CreateMessageReactionAdded(
        DiscordMessage message,
        DiscordUser user,
        DiscordChannel channel,
        DiscordEmoji emoji,
        DiscordGuild? guild = null)
    {
        var args = Create<MessageReactionAddedEventArgs>();
        SetReactionMembers(args, message, user, channel, emoji, guild);
        return args;
    }

    public static MessageReactionRemovedEventArgs CreateMessageReactionRemoved(
        DiscordMessage message,
        DiscordUser user,
        DiscordChannel channel,
        DiscordEmoji emoji,
        DiscordGuild? guild = null)
    {
        var args = Create<MessageReactionRemovedEventArgs>();
        SetReactionMembers(args, message, user, channel, emoji, guild);
        return args;
    }

    private static void SetReactionMembers(
        object args,
        DiscordMessage message,
        DiscordUser user,
        DiscordChannel channel,
        DiscordEmoji emoji,
        DiscordGuild? guild)
    {
        var resolvedGuild = guild ?? channel.Guild;
        TrySetMember(channel, "Guild", resolvedGuild);

        SetMember(args, "Message", message);
        SetMember(args, "User", user);
        TrySetMember(args, "Channel", channel);
        SetMember(args, "Emoji", emoji);
        TrySetMember(args, "Guild", resolvedGuild);
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
