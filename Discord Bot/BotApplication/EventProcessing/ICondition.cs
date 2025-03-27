namespace DiscordBot.BotApplication.EventProcessing;

public interface ICondition
{
    Task<bool> EvaluateAsync(object context);
}
