using Bot.SlashCommands.ResponseHelpers;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.SlashCommands
{
    public interface IRespondToButtonsAndModals : ISlashCommand
    {
        public Task OnRequestReceived(Request request);
    }
}
