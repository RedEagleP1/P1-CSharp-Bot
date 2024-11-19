using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using BotInfrastructure.HttpClients;
using Microsoft.Extensions.Options;

namespace Bot.Commands
{
    public class CreateDynamicCommands: ICreateDynamicCommands
    {
        //todo should include something to refresh the commands using createGuildCommand on the discord rest socket client
        private readonly IHttpClient _httpClient;
        private readonly IOptions<BotConfigurationModel> config;
        public List<string> _commands {get;set;} = new List<string>();
        public CreateDynamicCommands(IHttpClient httpClient, IOptions<BotConfigurationModel> config)
        {
            _httpClient = httpClient;
            this.config = config;
        }
        public async Task<ICollection<SlashCommandProperties>> BuildCommandAsync()
        {
            var commands = new List<SlashCommandProperties>();
            // Example of making an HTTP request to the endpoint
            var response = await _httpClient.GetAsync<CreateCommandModel[]>(config.Value.DiscordApiUrl);
            foreach (var command in response)
            {
                var newCommand = DynamicCommandBuilder.CreateCommand(command.Name, command.Description, command.Options).Build();
                commands.Add(newCommand);
                _commands.Add(command.Name);
            }
            // Process the response as needed and modify the command if necessary
            return commands;
        }
    }
}