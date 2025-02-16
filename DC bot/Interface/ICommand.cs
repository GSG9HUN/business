namespace DC_bot.Interface
{
    public interface ICommand
    {
        string Name { get; }
        string Description { get; }
        Task ExecuteAsync(IDiscordMessage message);
    }
}