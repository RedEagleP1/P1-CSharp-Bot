namespace DiscordBot.BotApplication.EventProcessing;
public interface ITrigger
{
    string Type { get; }
    bool Matches(object discordEvent);
}
