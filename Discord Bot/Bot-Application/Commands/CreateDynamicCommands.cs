using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using Bot_Infrastructure.HttpClients;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Bot_Application.Commands
{
    public class CreateDynamicCommands: ICreateDynamicCommands
    {
        //todo should include something to refresh the commands using createGuildCommand on the discord rest socket client
        private readonly IHttpClient _httpClient;
        public CreateDynamicCommands(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<ICollection<SlashCommandProperties>> BuildCommandAsync(string url)
        {
            var commands = new List<SlashCommandProperties>();
            var response = await _httpClient.GetAsync<CreateCommandModel[]>(url);
            foreach (var command in response)
            {
                var newCommand = DynamicCommandBuilder.CreateCommand(command.Name, command?.Description, command?.Options).Build();
                Debug.WriteLine($"Command: {newCommand.Name} has been created");
                commands.Add(newCommand);
            }
            return commands;
        }
    }
}