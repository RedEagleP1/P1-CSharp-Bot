namespace DiscordBot.BotApplication.Commands
{
    public class CommandContextContainer {
        public IList<string> CommandContexts { get; set; } = new List<string>();
        public CommandContextContainer() {
        }
    }
}