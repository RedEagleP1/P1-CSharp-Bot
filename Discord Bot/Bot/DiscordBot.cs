using System.Diagnostics;
using Bot.Config;
using Bot_Application.Commands;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace Bot
{
    public class DiscordBot : IDiscordBot {
        private DiscordSocketClient _discordSocketClient { get; set; }
        private readonly IOptions<Configuration> _configuration;
        private ICreateDynamicCommands _createDynamicCommands { get; set; }
        public DiscordBot(DiscordSocketClient discordSocketClient, IOptions<Configuration> configuration, ICreateDynamicCommands createDynamicCommands){
            _discordSocketClient = discordSocketClient;
            _configuration = configuration;
            _createDynamicCommands = createDynamicCommands;
        }

        public async Task StartAsync(){
            _discordSocketClient.Log += (message) =>
            {
                Debug.WriteLine(message.Message);
                return Task.CompletedTask;
            };

            var map = new Dictionary<string, DynamicCommandResponse>();

            _discordSocketClient.SlashCommandExecuted += async (interaction) => {

                var response = map[interaction.Data.Name];

                await response.RespondAsync();
            };
            
            await _discordSocketClient.LoginAsync(TokenType.Bot, _configuration.Value.Token);
            await _discordSocketClient.StartAsync();
            await Task.Delay(Timeout.Infinite);
        }
    }

}