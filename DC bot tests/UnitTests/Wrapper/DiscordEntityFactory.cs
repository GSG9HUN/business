using System.Runtime.CompilerServices;
using System.Reflection;
using DSharpPlus.Entities;

namespace DC_bot_tests.UnitTests.Wrapper;

internal static class DiscordEntityFactory
{
    private static T Create<T>() =>
        (T)RuntimeHelpers.GetUninitializedObject(typeof(T));

    private static void SetField(object obj, string fieldName, object? value)
    {
        var type = obj.GetType();
        FieldInfo? field = null;
        while (type != null && field == null)
        {
            field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            type = type.BaseType;
        }

        if (field == null)
            throw new InvalidOperationException($"Field '{fieldName}' not found on {obj.GetType().Name} or its base types");

        field.SetValue(obj, value);
    }

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

    public static DiscordUser CreateUser(ulong id = 1ul, string username = "TestUser", bool isBot = false)
    {
        var user = Create<DiscordUser>();
        SetField(user, "<Id>k__BackingField", id);
        SetField(user, "<Username>k__BackingField", username);
        SetField(user, "<IsBot>k__BackingField", isBot);
        return user;
    }

    public static DiscordChannel CreateChannel(ulong id = 1ul, string name = "test-channel", DiscordGuild? guild = null)
    {
        var channel = Create<DiscordChannel>();
        SetField(channel, "<Id>k__BackingField", id);
        SetField(channel, "<Name>k__BackingField", name);
        if (guild is not null)
        {
            TrySetMember(channel, "Guild", guild);
        }

        return channel;
    }

    public static DiscordMessage CreateMessage(
        ulong id = 1ul,
        string content = "test message",
        DiscordChannel? channel = null,
        DiscordUser? author = null)
    {
        var message = Create<DiscordMessage>();
        TrySetMember(message, "Id", id);
        TrySetMember(message, "Content", content);
        TrySetMember(message, "Channel", channel ?? CreateChannel());
        TrySetMember(message, "Author", author ?? CreateUser());
        TrySetMember(message, "CreationTimestamp", DateTimeOffset.UtcNow);
        TrySetMember(message, "embeds", new List<DiscordEmbed>());
        TrySetMember(message, "Embeds", new List<DiscordEmbed>());
        return message;
    }

    public static DiscordMember CreateMember(ulong id = 1ul, string username = "TestUser", bool isBot = false)
    {
        var member = Create<DiscordMember>();
        SetField(member, "<Id>k__BackingField", id);

        var internalUser = Create<DiscordUser>();
        SetField(internalUser, "<Id>k__BackingField", id);
        SetField(internalUser, "<Username>k__BackingField", username);
        SetField(internalUser, "<IsBot>k__BackingField", isBot);

        var userField = typeof(DiscordMember)
            .GetField("_user", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? typeof(DiscordMember)
                .GetField("<User>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

        if (userField != null)
            userField.SetValue(member, internalUser);

        var guild = Create<DiscordGuild>();
        SetField(guild, "<Id>k__BackingField", 0ul);
        var guildField = typeof(DiscordMember)
            .GetField("_guild", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? typeof(DiscordMember)
                .GetField("<Guild>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

        if (guildField != null)
            guildField.SetValue(member, guild);

        return member;
    }

    public static DiscordGuild CreateGuild(ulong id = 1ul, string name = "TestGuild")
    {
        var guild = Create<DiscordGuild>();
        SetField(guild, "<Id>k__BackingField", id);
        SetField(guild, "<Name>k__BackingField", name);
        return guild;
    }

    public static DiscordVoiceState CreateVoiceState(ulong? channelId = null)
    {
        var state = Create<DiscordVoiceState>();
        SetField(state, "<ChannelId>k__BackingField", channelId);
        return state;
    }
}
