using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using Bot_Infrastructure.HttpClients;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using Microsoft.VisualBasic;

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
            var response = TestData.GetTestCommands(); //await _httpClient.GetAsync<CreateCommandModel[]>(url);
            foreach (var command in response)
            {
                var newCommand = DynamicCommandBuilder.CreateCommand(command.Name, command?.Description, command?.Options).Build();
                Debug.WriteLine($"Command: {newCommand.Name} has been created");
                commands.Add(newCommand);
            }
            return commands;
        }

        private async Task<IList<DynamicCommandResponse>> CreateResponsesAsync(string url)
        {
            var responses = await _httpClient.GetAsync<CreateResponseModel[]>(url);
            var commandResponses = new List<DynamicCommandResponse>();
            foreach (var response in responses)
            {
                var newResponse = DynamicCommandResponseBuilder.CreateResponse(response.Title, response.Description, response.Content, response.Color, response.IsEphemeral, response.IsTTS);
                commandResponses.Add(newResponse);
                Debug.WriteLine($"Response: {response.Title} has been created");
            }
            return commandResponses;
        }
    }

    public class CreateResponseModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public Color? Color { get; set; }
        public bool IsEphemeral { get; set; }
        public bool IsTTS { get; set; }
    }
}