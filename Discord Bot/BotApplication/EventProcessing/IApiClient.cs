namespace DiscordBot.BotApplication.EventProcessing;

public interface IApiClient
{
    Task<IEnumerable<RuleDefinition<TResponse>>> FetchRulesAsync<TResponse>();
    Task<T> SendEventContextAsync<T>(EventContext context);
}
