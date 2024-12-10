using Discord;
using Microsoft.Extensions.Options;

namespace Bot_Application.Commands
{
    public interface ICreateDynamicCommands {
        public Task<ICollection<SlashCommandProperties>> BuildCommandAsync(string url);
    }
}