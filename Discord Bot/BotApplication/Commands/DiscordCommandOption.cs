using Discord;

namespace DiscordBot.BotApplication.Commands
{
    public class DiscordCommandOption
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public ApplicationCommandOptionType Type { get; set; }
        public bool Required { get; internal set; }
        public List<DiscordCommandOption>? Options { get; set; }
    }
}