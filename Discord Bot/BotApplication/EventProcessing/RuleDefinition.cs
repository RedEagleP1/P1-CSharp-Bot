namespace DiscordBot.BotApplication.EventProcessing
{
    public class RuleDefinition<TResponse>
    {
        public string Id { get; set; }
        public ITrigger Trigger { get; set; }
        public ICondition Condition { get; set; }
    }
}
