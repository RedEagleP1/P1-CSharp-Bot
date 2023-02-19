using Bot.SlashCommands;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

namespace Bot.EventHandlers
{
    public class SlashCommandHandler : IEventHandler
    {
        readonly DiscordSocketClient client;
        readonly List<ISlashCommand> slashCommands;
        public SlashCommandHandler(DiscordSocketClient client, List<ISlashCommand> slashCommands)
        {
            this.client = client;
            this.slashCommands = slashCommands;
        }

        public void Subscribe()
        {
            client.SlashCommandExecuted += OnSlashCommandExecuted;

            foreach(var command in slashCommands)
            {
                if(command is INeedAwake awake)
                {
                    awake.Awake();
                }
            }
        }

        private async Task OnSlashCommandExecuted(SocketSlashCommand command)
        {
            foreach (var slashCommand in slashCommands)
            {
                if (slashCommand.Name == command.Data.Name)
                {
                    await slashCommand.HandleCommand(command);
                    return;
                }
            }
        }
    }
}
