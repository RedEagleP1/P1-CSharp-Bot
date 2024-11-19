using Discord;
using Microsoft.Extensions.Options;

namespace Bot.Commands
{
    public interface ICreateDynamicCommands {
        // TODO: Rename to something generic like `Handle`
        public Task<ICollection<SlashCommandProperties>> BuildCommandAsync();
    }
}