# Discord

Groups the upstream Discord REST payloads used by authentication services.

## Overview

- DiscordOAuthService deserializes token, current-user, and guild responses from the Response subfolder.
- These are Discord transport shapes, not mobile HTTP responses or persistence entities. Keep provider field names and optional values aligned with their consumers.

## Subfolders

### Response/

Represents token, user, and guild data returned by Discord REST requests.

**Files:**

- `DiscordGuildResponse.cs`
- `DiscordTokenResponse.cs`
- `DiscordUserResponse.cs`

**Purpose:** The OAuth service decodes these fields, and AuthHandler converts identity and guild data into shared persistence input records.

---

## Classes

### DiscordGuildResponse

**File:** `API/Services/Auth/Discord/Response/DiscordGuildResponse.cs`

**Purpose:** Deserializes Discord OAuth guild fields returned by the Discord REST API.

**Properties:**

```csharp
[JsonPropertyName("id")]
public string Id { get; set; }
[JsonPropertyName("name")]
public string Name { get; set; }
[JsonPropertyName("owner")]
public bool Owner { get; set; }
[JsonPropertyName("permissions")]
[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
public ulong Permissions { get; set; }
[JsonPropertyName("icon")]
public string? Icon { get; set; }
```

---

### DiscordTokenResponse

**File:** `API/Services/Auth/Discord/Response/DiscordTokenResponse.cs`

**Purpose:** Deserializes Discord OAuth token fields returned by the Discord REST API.

**Properties:**

```csharp
[JsonPropertyName("access_token")]
public string AccessToken { get; set; }
[JsonPropertyName("token_type")]
public string TokenType { get; set; }
[JsonPropertyName("expires_in")]
public int ExpiresIn { get; set; }
[JsonPropertyName("refresh_token")]
public string? RefreshToken { get; set; }
[JsonPropertyName("scope")]
public string Scope { get; set; }
```

---

### DiscordUserResponse

**File:** `API/Services/Auth/Discord/Response/DiscordUserResponse.cs`

**Purpose:** Deserializes Discord OAuth user fields returned by the Discord REST API.

**Properties:**

```csharp
[JsonPropertyName("id")]
public string Id { get; set; }
[JsonPropertyName("username")]
public string Username { get; set; }
[JsonPropertyName("global_name")]
public string? GlobalName { get; set; }
[JsonPropertyName("avatar")]
public string? Avatar { get; set; }
```

---

## Related Components

- `API/Services/Auth/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `API/Services/Auth/Discord/Response/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
