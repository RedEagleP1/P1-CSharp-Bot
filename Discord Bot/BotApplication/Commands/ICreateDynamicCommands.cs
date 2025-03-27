using Discord;

namespace DiscordBot.BotApplication.Commands
{
    public interface ICreateDynamicCommands {
        public Task<ICollection<SlashCommandProperties>> BuildCommandAsync(string url);
    }
}