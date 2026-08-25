using System.Text;
using API.Endpoints;
using API.Services.Auth;
using DC_bot.DependencyInjection;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

LoadDotEnv();

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["AppAuth:SigningKey"] = GetRequiredEnvironmentVariable("APP_AUTH_SIGNING_KEY"),
    ["AppAuth:Issuer"] = GetRequiredEnvironmentVariable("APP_AUTH_ISSUER"),
    ["AppAuth:Audience"] = GetRequiredEnvironmentVariable("APP_AUTH_AUDIENCE"),
    ["DiscordOAuth:ClientId"] = GetRequiredEnvironmentVariable("DISCORD_OAUTH_CLIENT_ID"),
    ["DiscordOAuth:ClientSecret"] = GetRequiredEnvironmentVariable("DISCORD_OAUTH_CLIENT_SECRET"),
    ["DiscordOAuth:RedirectUri"] = GetRequiredEnvironmentVariable("DISCORD_OAUTH_REDIRECT_URI"),
    ["DiscordOAuth:AndroidRedirectUri"] = GetRequiredEnvironmentVariable("DISCORD_OAUTH_ANDROID_REDIRECT_URI")
});

var postgresHost = GetRequiredEnvironmentVariable("POSTGRES_HOST");
var postgresPort = GetRequiredEnvironmentVariable("POSTGRES_PORT");
var postgresDb = GetRequiredEnvironmentVariable("POSTGRES_DB");
var postgresUser = GetRequiredEnvironmentVariable("POSTGRES_USER");
var postgresPassword = GetRequiredEnvironmentVariable("POSTGRES_PASSWORD");

var postgresConnectionString =
    $"Host={postgresHost};Port={postgresPort};Database={postgresDb};Username={postgresUser};Password={postgresPassword}";

builder.Services.AddOpenApi();
builder.Services.AddPersistenceServices(postgresConnectionString);
builder.Services.AddMemoryCache();

builder.Services.AddHttpClient<DiscordOAuthService>();
builder.Services.AddSingleton<AppTokenService>()
    .AddSingleton<OAuthStateStore>()
    .AddSingleton<AuthTicketStore>();

var jwtKey = builder.Configuration["AppAuth:SigningKey"]
             ?? throw new InvalidOperationException("Missing AppAuth:SigningKey.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["AppAuth:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["AppAuth:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });


builder.Services.AddAuthorization();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

var api = app.MapGroup("/api");
api.MapGuildEndpoints()
    .MapProfileEndpoints()
    .MapAuthEndpoints()
    .MapPlayerEndpoints()
    .MapPlaybackEndpoints()
    .MapPlaylistEndpoints()
    .MapStatusEndpoints()
    .MapQueueEndpoints();

app.Run();

static void LoadDotEnv()
{
    foreach (var envPath in GetDotEnvCandidates())
    {
        if (!File.Exists(envPath))
        {
            continue;
        }

        Env.NoClobber().Load(envPath);
        return;
    }
}

static IEnumerable<string> GetDotEnvCandidates()
{
    foreach (var startDirectory in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
    {
        var directory = new DirectoryInfo(startDirectory);

        while (directory is not null)
        {
            yield return Path.Combine(directory.FullName, ".env");
            directory = directory.Parent;
        }
    }
}

static string GetRequiredEnvironmentVariable(string name)
{
    var value = Environment.GetEnvironmentVariable(name);

    return string.IsNullOrWhiteSpace(value)
        ? throw new InvalidOperationException($"Missing required environment variable: {name}.")
        : value;
}
