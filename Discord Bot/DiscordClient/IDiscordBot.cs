namespace DiscordClient
{
    public interface IDiscordBot {
        Task StartAsync();
        Task RestartAsync();
        Task CloseAsync();
    }
}