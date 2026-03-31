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
    Task SendMessageAsync(string message);
    IDiscordGuild Guild { get; }
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
    string Username { get; }
    bool IsBot { get; }
}
```

**Implementation:** `Wrapper/DiscordUserWrapper.cs`

---

### IDiscordMember.cs

```csharp
public interface IDiscordMember : IDiscordUser
{
    IDiscordGuild Guild { get; }
    IDiscordVoiceState? VoiceState { get; }
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
}
```

**Implementation:** `Wrapper/DiscordGuildWrapper.cs`

---

### IDiscordVoiceState.cs

```csharp
public interface IDiscordVoiceState
{
    IDiscordChannel? Channel { get; }
}
```

**Implementation:** `Wrapper/DiscordVoiceStateWrapper.cs`

---

## Related Components

- **Wrapper/** - Implements these interfaces
- **Commands/** - Use these interfaces instead of DSharpPlus types
- **Service/** - Use these interfaces for Discord operations

