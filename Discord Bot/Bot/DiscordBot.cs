using System.Diagnostics;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace Bot
{
    public class DiscordBot : IDiscordBot {
        private DiscordSocketClient _discordSocketClient { get; set; }
        private readonly BotConfigurationModel _configuration;
        public DiscordBot(DiscordSocketClient discordSocketClient, IOptions<BotConfigurationModel> configuration){
            _discordSocketClient = discordSocketClient;
            _configuration = configuration.Value;
        }

        public async Task Start(){
            await _discordSocketClient.StartAsync();
            await _discordSocketClient.LoginAsync(TokenType.Bot, _configuration.Token);
            await Task.Delay(Timeout.Infinite);
        }
    }

    public interface IDiscordBot {
        Task Start();
    }

}