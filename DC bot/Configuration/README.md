# Configuration

This folder contains configuration models and settings classes.

## What's here?

Configuration classes that represent structured settings loaded from:
- Environment variables
- `.env` files
- Command-line arguments

These classes use the **Options Pattern** in ASP.NET Core for type-safe configuration.

## Configuration Classes

### BotSettings.cs
Core bot configuration:
```csharp
public sealed class BotSettings
{
    public string? Token { get; init; }      // Discord bot token
    public string Prefix { get; init; }      // Command prefix (default: "!")
}
```

**Usage:**
```csharp
services.Configure<BotSettings>(configuration.GetSection("Bot"));

// Inject via IOptions<BotSettings>
public class MyService(IOptions<BotSettings> botOptions)
{
    private readonly BotSettings _settings = botOptions.Value;
    
    public void DoSomething()
    {
        var token = _settings.Token;
        var prefix = _settings.Prefix;
    }
}
```

### LavalinkSettings.cs
Lavalink audio server configuration:
```csharp
public sealed class LavalinkSettings
{
    public string Hostname { get; init; }    // Lavalink server hostname
    public int Port { get; init; }           // Lavalink server port
    public string Password { get; init; }    // Lavalink server password
}
```

**Usage:**
```csharp
services.Configure<LavalinkSettings>(configuration.GetSection("Lavalink"));

// Automatically injected into Lavalink4NET services
```

## Configuration Sources

### .env File
```env
BOT__TOKEN=your_discord_bot_token_here
BOT__PREFIX=!
LAVALINK__HOSTNAME=localhost
LAVALINK__PORT=2333
LAVALINK__PASSWORD=youshallnotpass
```

**Note:** Double underscore `__` represents nested configuration.

### Environment Variables (Production)
```bash
export BOT__TOKEN="production_token"
export LAVALINK__HOSTNAME="lavalink.example.com"
export LAVALINK__PORT="2333"
export LAVALINK__PASSWORD="secure_password"
```

## Loading Configuration

Configuration is loaded in `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Load configuration from multiple sources
builder.Configuration
    .AddEnvironmentVariables()
    .AddCommandLine(args);

// Bind configuration to strongly-typed classes
builder.Services.Configure<BotSettings>(
    builder.Configuration.GetSection("Bot"));
    
builder.Services.Configure<LavalinkSettings>(
    builder.Configuration.GetSection("Lavalink"));
```

## Configuration Priority (Highest to Lowest)

1. **Command-line arguments** - `--Bot:Token=xyz`
2. **Environment variables** - `BOT__TOKEN=xyz`
3. **Default values** - Hardcoded in classes

## Validation

Configuration can be validated on startup:

```csharp
services.AddOptions<BotSettings>()
    .Bind(configuration.GetSection("Bot"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// BotSettings.cs with validation
public sealed class BotSettings
{
    [Required]
    [MinLength(50)]
    public string Token { get; init; } = string.Empty;
    
    [Required]
    [RegularExpression(@"^[!@#$%^&*]$")]
    public string Prefix { get; init; } = "!";
}
```

## Security Best Practices

- ✅ **Never** commit secrets to source control
- ✅ Use `.env` files for local development (add to `.gitignore`)
- ✅ Use environment variables in production
- ✅ Use Azure Key Vault / AWS Secrets Manager for cloud deployments
- ✅ Mark sensitive properties as `internal` or `private`
- ❌ Don't log sensitive configuration values
- ❌ Don't expose tokens in error messages

## Example .gitignore

```gitignore
appsettings.Development.json
appsettings.Production.json
.env
.env.local
*.user
appsettings.*.json
!appsettings.json
```

## Adding New Configuration

To add new configuration:

1. **Create the class:**
```csharp
public sealed class NewFeatureSettings
{
    public bool Enabled { get; init; }
    public int MaxRetries { get; init; } = 3;
}
```

2. **Register in Program.cs:**
```csharp
builder.Services.Configure<NewFeatureSettings>(
    builder.Configuration.GetSection("NewFeature"));
```

3. **Inject where needed:**
```csharp
public class MyService(IOptions<NewFeatureSettings> options)
{
    private readonly NewFeatureSettings _settings = options.Value;
}
```

## Options Pattern Benefits

- ✅ **Type safety** - Compile-time checking
- ✅ **IntelliSense** - Auto-completion in IDE
- ✅ **Validation** - Data annotations support
- ✅ **Reloading** - `IOptionsSnapshot<T>` for hot reload
- ✅ **Testing** - Easy to mock with `Options.Create()`
- ✅ **Separation** - Different configs for different features

## Related

- **Program.cs** - Configuration loading and service registration
- **.env** - Local development secrets (gitignored)

