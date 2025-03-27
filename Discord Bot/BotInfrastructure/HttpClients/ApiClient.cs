using DiscordBot.BotApplication.EventProcessing;

namespace DiscordBot.BotInfrastructure.HttpClients;

public class ApiClient : IApiClient {

    private readonly BotHttpClient _botHttpClient;

    public ApiClient(BotHttpClient botHttpClient)
    {
        _botHttpClient = botHttpClient;
    }

    public Task<IEnumerable<RuleDefinition<TResponse>>> FetchRulesAsync<TResponse>()
    {
        throw new NotImplementedException();
    }

    public Task<T> SendEventContextAsync<T>(EventContext context)
    {
        throw new NotImplementedException();
    }
}