using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using BotInfrastructure.HttpClients;

namespace Bot.Commands
{
    public class CreateDynamicCommands
    {
        private readonly IHttpClient _httpClient;
        public CreateDynamicCommands(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<ICollection<SlashCommandProperties>> BuildCommandAsync(string name, string description, string endpoint, List<SlashCommandOptionBuilder> options = null)
        {
            var commands = new List<SlashCommandProperties>();
            // Example of making an HTTP request to the endpoint
            var response = await _httpClient.GetAsync<CreateCommandModel[]>(endpoint);
            foreach (var command in response) {
                var newCommand = DynamicCommandBuilder.CreateCommand(command.Name, command.Description, command.Options).Build();
                commands.Add(newCommand);
            }
            // Process the response as needed and modify the command if necessary
            return commands;
        }
    }
}