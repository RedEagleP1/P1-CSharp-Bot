using Discord;

namespace Bot.Commands
{
    public class DiscordCommandOption
    {
        public string Name { get; set; }
        public string Description { get; set; }
        // todo need a converter/type resolver
        public ApplicationCommandOptionType Type { get; set; }
        public bool Required { get; internal set; }
        public List<DiscordCommandOption> Options { get; set; }
    }
}