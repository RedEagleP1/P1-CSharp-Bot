using Discord;
using Microsoft.Extensions.Options;

namespace Bot_Application.Commands
{
    public interface ICreateDynamicCommands {
        // TODO: Rename to something generic like `Handle`
        public Task<ICollection<SlashCommandProperties>> BuildCommandAsync(string url);
    }
}