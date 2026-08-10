using System.Text;
using API.Endpoints;
using API.Services.Auth;
using DC_bot.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var postgresHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
var postgresPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
var postgresDb = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "dc_bot";
var postgresUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres";
var postgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres";

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
    .MapAuthEndpoints()
    .MapPlayerEndpoints()
    .MapPlaybackEndpoints()
    .MapPlaylistEndpoints()
    .MapStatusEndpoints()
    .MapQueueEndpoints();

app.Run();
