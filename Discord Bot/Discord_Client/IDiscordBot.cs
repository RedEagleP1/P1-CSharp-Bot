namespace Discord_Client
{
    public interface IDiscordBot {
        Task StartAsync();
        Task RestartAsync();
        Task CloseAsync();
    }
}