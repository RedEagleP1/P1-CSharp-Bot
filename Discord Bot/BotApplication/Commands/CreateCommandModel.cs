using Discord;

namespace DiscordBot.BotApplication.Commands
{
    public class CreateCommandModel
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public List<DiscordCommandOption>? Options { get; set; }
    }
}
