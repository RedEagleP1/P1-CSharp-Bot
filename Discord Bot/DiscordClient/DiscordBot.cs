using System.Diagnostics;
using Discord;
using Discord.WebSocket;
using DiscordBot.BotApplication.Commands;
using DiscordClient.Config;
using DiscordClient.Services;
using Microsoft.Extensions.Options;

namespace DiscordClient
{
    public class DiscordBot : IDiscordBot
    {
        private DiscordSocketClient _discordSocketClient { get; set; }
        private readonly IOptions<Configuration> _configuration;
        private ICreateDynamicCommands _createDynamicCommands { get; set; }
        private ResponseCache _responseCache { get; set; }
        public DiscordBot(DiscordSocketClient discordSocketClient, IOptions<Configuration> configuration, ICreateDynamicCommands createDynamicCommands, ResponseCache responseCache)
        {
            _discordSocketClient = discordSocketClient;
            _configuration = configuration;
            _createDynamicCommands = createDynamicCommands;
            _responseCache = responseCache;
        }

        public async Task StartAsync()
        {
            _discordSocketClient.Log += (message) =>
            {
                Debug.WriteLine(message.Message);
                return Task.CompletedTask;
            };
            
            RegisterCommands();
            await _discordSocketClient.LoginAsync(TokenType.Bot, _configuration.Value.Token);
            await _discordSocketClient.StartAsync();
            await Task.Delay(Timeout.Infinite);
        }

        public async Task RestartAsync()
        {
            await _discordSocketClient.StopAsync();
            await _discordSocketClient.StartAsync();
        }

        public async Task CloseAsync()
        {
            await _discordSocketClient.StopAsync();
            await _discordSocketClient.LogoutAsync();
        }

        public async Task ReloadResponseCacheAsync()
        {
            await _responseCache.ReloadAsync();
        }

        private void RegisterCommands()
        {
            _discordSocketClient.Ready += CreateCommands;
            //todo Add element to distinguish between the event that it should be registered.
            _discordSocketClient.SlashCommandExecuted += async (interaction) =>
            {
                if (interaction.Data.Name == "message" || interaction.Data.Name == "user") {
                    var response = _responseCache.Get("test") as DynamicCommandResponse;
                    if (response != null) await response.RespondAsync(interaction);
                };
            };
        }

        private async Task CreateCommands()
        {
            //todo Add distinquishing element to determine if the command is a global or guild command
            var commands = await _createDynamicCommands.BuildCommandAsync(_configuration.Value.BaseUrl + _configuration.Value.CommandRoute);
            foreach (var command in commands)
            {
                //todo this is for development only, change this to be dynamic (handle global and guild commands)
                ulong guildId = 614851901846192128;
                var guild = _discordSocketClient.GetGuild(guildId);
                await guild.CreateApplicationCommandAsync(command);
            }
        }
    }

}