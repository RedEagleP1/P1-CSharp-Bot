using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.SlashCommands
{
    public interface ISlashCommand
    {
        public string Name { get; }
        public SlashCommandProperties Properties { get; }
        public Task HandleCommand(SocketSlashCommand command);
    }
}
