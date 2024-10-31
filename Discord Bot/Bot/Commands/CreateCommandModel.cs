using Discord;

namespace Bot.Commands
{
    public class CreateCommandModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Endpoint { get; set; }
        public List<SlashCommandOptionBuilder> Options { get; set; }
    }
}
