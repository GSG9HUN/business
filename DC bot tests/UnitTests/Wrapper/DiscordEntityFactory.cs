using System.Runtime.CompilerServices;
using System.Reflection;
using DSharpPlus.Entities;

namespace DC_bot_tests.UnitTests.Wrapper;

/// <summary>
/// Segédosztály DSharpPlus sealed/internal entitások létrehozásához
/// reflection alapon, konstruktor és setter hozzáférés nélkül.
/// </summary>
internal static class DiscordEntityFactory
{
    private static T Create<T>() =>
        (T)RuntimeHelpers.GetUninitializedObject(typeof(T));

    /// <summary>
    /// Végigmegy az öröklési láncon hogy megtalálja a backing fieldet
    /// (pl. Id a SnowflakeObject-ben van, nem a leszármazottban)
    /// </summary>
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

    public static DiscordUser CreateUser(ulong id = 1ul, string username = "TestUser", bool isBot = false)
    {
        var user = Create<DiscordUser>();
        SetField(user, "<Id>k__BackingField", id);
        SetField(user, "<Username>k__BackingField", username);
        SetField(user, "<IsBot>k__BackingField", isBot);
        return user;
    }

    public static DiscordChannel CreateChannel(ulong id = 1ul, string name = "test-channel")
    {
        var channel = Create<DiscordChannel>();
        SetField(channel, "<Id>k__BackingField", id);
        SetField(channel, "<Name>k__BackingField", name);
        return channel;
    }

    public static DiscordMember CreateMember(ulong id = 1ul, string username = "TestUser", bool isBot = false)
    {
        var member = Create<DiscordMember>();
        // Id az örökölt SnowflakeObject-ben van
        SetField(member, "<Id>k__BackingField", id);

        // DiscordMember.Username/IsBot egy belső _user DiscordUser-en keresztül érhető el
        var internalUser = Create<DiscordUser>();
        SetField(internalUser, "<Id>k__BackingField", id);
        SetField(internalUser, "<Username>k__BackingField", username);
        SetField(internalUser, "<IsBot>k__BackingField", isBot);

        // A _user field a DiscordMember-ben
        var userField = typeof(DiscordMember)
            .GetField("_user", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? typeof(DiscordMember)
                .GetField("<User>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

        if (userField != null)
            userField.SetValue(member, internalUser);

        // VoiceState-hez szükség van a Guild field-re (_guild) is
        // Ha nincs guild, a VoiceState getter NullReferenceException-t dob
        // Ezért egy üres DiscordGuild-et adunk
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
