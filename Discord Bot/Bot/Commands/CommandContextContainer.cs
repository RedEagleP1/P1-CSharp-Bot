namespace Bot.Commands {

    public class CommandContextContainer {
        public IList<string> CommandContexts { get; set; } = new List<string>();
        public CommandContextContainer() {

        }
    }
}