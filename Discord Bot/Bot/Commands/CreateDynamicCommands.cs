using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using BotInfrastructure.HttpClients;
using Microsoft.Extensions.Options;

namespace Bot.Commands
{
    public interface ICreateDynamicCommands {
        // TODO: Rename to something generic like `Handle`
        public Task<ICollection<SlashCommandProperties>> BuildCommandAsync(IOptions<BotConfigurationModel> config);
    }

    public class CreateDynamicCommands: ICreateDynamicCommands
    {
        //todo should include something to refresh the commands using createGuildCommand on the discord rest socket client
        private readonly IHttpClient _httpClient;
        public CreateDynamicCommands(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<ICollection<SlashCommandProperties>> BuildCommandAsync(IOptions<BotConfigurationModel> config)
        {
            var commands = new List<SlashCommandProperties>();
            // Example of making an HTTP request to the endpoint
            var response = await _httpClient.GetAsync<CreateCommandModel[]>(config.Value.DiscordApiUrl);
            foreach (var command in response)
            {
                var newCommand = DynamicCommandBuilder.CreateCommand(command.Name, command.Description, command.Options).Build();
                commands.Add(newCommand);
            }
            // Process the response as needed and modify the command if necessary
            return commands;
        }
    }
}