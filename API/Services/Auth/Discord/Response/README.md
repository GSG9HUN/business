# Response

Represents token, user, and guild data returned by Discord REST requests.

## Overview

- The OAuth service decodes these fields, and AuthHandler converts identity and guild data into shared persistence input records.
- Discord bearer tokens remain server-side. Mobile sessions use application tokens created separately by AppTokenService.

## Classes

### DiscordGuildResponse

**File:** `API/Services/Auth/Discord/Response/DiscordGuildResponse.cs`

**Purpose:** Deserializes Discord OAuth guild fields returned by the Discord REST API.

**Properties:**

```csharp
[JsonPropertyName("id")]
// The Discord guild identifier serialized as a string.
public string Id { get; set; }
[JsonPropertyName("name")]
// The display name.
public string Name { get; set; }
[JsonPropertyName("owner")]
// Whether the authenticated Discord user owns the guild.
public bool Owner { get; set; }
[JsonPropertyName("permissions")]
[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
// The Discord permission bit field for the user's guild membership.
public ulong Permissions { get; set; }
[JsonPropertyName("icon")]
// The Discord guild icon hash, or null when no icon is set.
public string? Icon { get; set; }
```

---

### DiscordTokenResponse

**File:** `API/Services/Auth/Discord/Response/DiscordTokenResponse.cs`

**Purpose:** Deserializes Discord OAuth token fields returned by the Discord REST API.

**Properties:**

```csharp
[JsonPropertyName("access_token")]
// The access token returned by Discord OAuth.
public string AccessToken { get; set; }
[JsonPropertyName("token_type")]
// The authorization scheme returned with the access token.
public string TokenType { get; set; }
[JsonPropertyName("expires_in")]
// The access token lifetime reported by Discord, in seconds.
public int ExpiresIn { get; set; }
[JsonPropertyName("refresh_token")]
// The refresh token returned by Discord, or null when absent.
public string? RefreshToken { get; set; }
[JsonPropertyName("scope")]
// The OAuth scopes granted by Discord.
public string Scope { get; set; }
```

---

### DiscordUserResponse

**File:** `API/Services/Auth/Discord/Response/DiscordUserResponse.cs`

**Purpose:** Deserializes Discord OAuth user fields returned by the Discord REST API.

**Properties:**

```csharp
[JsonPropertyName("id")]
// The Discord guild identifier serialized as a string.
public string Id { get; set; }
[JsonPropertyName("username")]
// The Discord username.
public string Username { get; set; }
[JsonPropertyName("global_name")]
// The Discord global display name, or null when it is not set.
public string? GlobalName { get; set; }
[JsonPropertyName("avatar")]
// The Discord avatar hash, or null when no custom avatar is set.
public string? Avatar { get; set; }
```

---

## Related Components

- `API/Services/Auth/Discord/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
