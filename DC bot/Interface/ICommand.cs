using System.Threading.Tasks;
using Discord.WebSocket;

namespace DC_bot.Interface
{
    public interface ICommand
    {
        string Name { get; }
        Task ExecuteAsync(SocketMessage message);
    }
}