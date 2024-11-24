using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using IDiscordClient = DC_bot.Interface.IDiscordClient;

namespace DC_bot.Wrapper;

public class DiscordClientWrapper : IDiscordClient
{
    private readonly DiscordSocketClient _client;

    public DiscordClientWrapper()
    {
        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        };
            
        _client = new DiscordSocketClient(config);
    }

    public Task LoginAsync(TokenType tokenType, string token)
        => _client.LoginAsync(tokenType, token);

    public Task StartAsync()
        => _client.StartAsync();

    public event Func<SocketMessage, Task> MessageReceived
    {
        add => _client.MessageReceived += value;
        remove => _client.MessageReceived -= value;
    }

    public event Func<LogMessage, Task> Log
    {
        add => _client.Log += value;
        remove => _client.Log -= value;
    }
}