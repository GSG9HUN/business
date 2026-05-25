# Discord Interfaces

This folder contains Discord abstraction interfaces.

## Overview

These interfaces abstract the DSharpPlus library, making the code:

- Easier to test (mockable)
- Less coupled to DSharpPlus
- Portable to other Discord libraries

## Files

### IDiscordMessage.cs

```csharp
public interface IDiscordMessage
{
    ulong Id { get; set; }
    string Content { get; set; }
    IDiscordChannel Channel { get; set; }
    IDiscordUser Author { get; set; }
    DateTimeOffset CreatedAt { get; set; }
    IReadOnlyList<DiscordEmbed> Embeds { get; set; }
    Task RespondAsync(string message);
    Task RespondAsync(DiscordEmbed message);
    Task ModifyAsync(DiscordMessageBuilder builder);
}
```

**Implementation:** `Wrapper/DiscordMessageWrapper.cs`

---

### IDiscordChannel.cs

```csharp
public interface IDiscordChannel
{
    ulong Id { get; }
    string Name { get; }
    IDiscordGuild Guild { get; }
    Task SendMessageAsync(string message);
    Task SendMessageAsync(DiscordEmbed embed);
    DiscordChannel ToDiscordChannel();
}
```

**Implementation:** `Wrapper/DiscordChannelWrapper.cs`

---

### IDiscordUser.cs

```csharp
public interface IDiscordUser
{
    ulong Id { get; }
    bool IsBot { get; }
    string Username { get; }
    string Mention { get; }
    DiscordUser ToDiscordUser();
}
```

**Implementation:** `Wrapper/DiscordUserWrapper.cs`

---

### IDiscordMember.cs

```csharp
public interface IDiscordMember
{
    ulong Id { get; }
    bool IsBot { get; }
    string Username { get; }
    string Mention { get; }
    IDiscordVoiceState? VoiceState { get; }
    DiscordMember ToDiscordMember();
}
```

**Implementation:** `Wrapper/DiscordMemberWrapper.cs`

---

### IDiscordGuild.cs

```csharp
public interface IDiscordGuild
{
    ulong Id { get; }
    string Name { get; }
    Task<IDiscordMember> GetMemberAsync(ulong id);
    DiscordGuild ToDiscordGuild();
    Task<IReadOnlyCollection<IDiscordMember>> GetAllMembersAsync();
}
```

**Implementation:** `Wrapper/DiscordGuildWrapper.cs`

---

### IDiscordVoiceState.cs

```csharp
public interface IDiscordVoiceState
{
    IDiscordChannel? Channel { get; }
    DiscordVoiceState ToDiscordVoiceState();
}
```

**Implementation:** `Wrapper/DiscordVoiceStateWrapper.cs`

---

## Related Components

- **Wrapper/** - Implements these interfaces
- **Commands/** - Use these interfaces instead of DSharpPlus types
- **Service/** - Use these interfaces for Discord operations

