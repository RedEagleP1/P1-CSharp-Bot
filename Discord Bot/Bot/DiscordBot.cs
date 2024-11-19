using System.Diagnostics;
using Bot.Commands;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace Bot
{
    public class DiscordBot : IDiscordBot {
        private DiscordSocketClient _discordSocketClient { get; set; }
        private readonly BotConfigurationModel _configuration;
        private DiscordEventListener _listener { get; set; }
        private CreateDynamicCommands _createDynamicCommands { get; set; }
        private CommandContextContainer _commandContextContainer { get; set; }
        public DiscordBot(DiscordEventListener listener, DiscordSocketClient discordSocketClient, IOptions<BotConfigurationModel> configuration, CreateDynamicCommands createDynamicCommands, CommandContextContainer commandContextContainer){
            _discordSocketClient = discordSocketClient;
            _configuration = configuration.Value;
            _listener = listener;
            _createDynamicCommands = createDynamicCommands;
            _commandContextContainer = commandContextContainer;
        }

        public async Task StartAsync(){
            
            await _listener.StartAsync();
            
            await _discordSocketClient.LoginAsync(TokenType.Bot, _configuration.Token);
            await _discordSocketClient.StartAsync();
            await _createDynamicCommands.BuildCommandAsync();
            _commandContextContainer.CommandContexts = _createDynamicCommands._commands;
            
            await Task.Delay(Timeout.Infinite);
        }
    }

    public interface IDiscordBot {
        Task StartAsync();
    }

}