namespace DiscordBot.BotApplication.EventProcessing;

public class EventContext
{
    public string RuleId { get; set; }
    public string EventType { get; set; }
    public Dictionary<string, object> EventData { get; set; }
    public DateTime Timestamp { get; set; }
}
