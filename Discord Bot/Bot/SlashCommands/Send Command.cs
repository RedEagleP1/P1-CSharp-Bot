
using Bot.SlashCommands.ResponseHelpers;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Bot.SlashCommands
{
    internal class SendCommand : ISlashCommand
    {
        public string Name => throw new NotImplementedException();

        public SlashCommandProperties Properties => throw new NotImplementedException();

        public Task HandleCommand(SocketSlashCommand command)
        {
            throw new NotImplementedException();
        }





    }

}


