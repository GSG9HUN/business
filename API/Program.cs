using API.Endpoints;
using DC_bot.DependencyInjection;

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var api = app.MapGroup("/api");
api.MapGuildEndpoints()
    .MapPlayerEndpoints()
    .MapPlaybackEndpoints()
    .MapPlaylistEndpoints()
    .MapQueueEndpoints();
app.Run();
