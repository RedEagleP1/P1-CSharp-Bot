using Discord.WebSocket;

namespace DiscordBot.BotApplication.EventProcessing;

public class DiscordEventHandler
{
    private readonly IApiClient _apiClient;
    private readonly Dictionary<string, List<object>> _rulesByTriggerType;

    public DiscordEventHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
        _rulesByTriggerType = new Dictionary<string, List<object>>();
    }

    // todo Change the current rules by trigger type to handle thread safety
    public void RegisterRuleAsync<TResponse>(RuleDefinition<TResponse> rule)
    {
        var triggerType = rule.Trigger.Type;
        if (!_rulesByTriggerType.ContainsKey(triggerType))
        {
            _rulesByTriggerType[triggerType] = new List<object>();
        }
        _rulesByTriggerType[triggerType].Add(rule);
    }

    public async Task HandleEventAsync(object discordEvent)
    {
        var eventType = DetermineEventType(discordEvent);

        if (!_rulesByTriggerType.TryGetValue(eventType, out var rules))
            return;

        foreach (var rule in rules)
        {
            await ProcessRuleAsync(rule, discordEvent);
        }
    }

    // TODO create an interface for the rule definition types or refactor this method to be more, possibly reflection
    private async Task ProcessRuleAsync(object ruleObj, object discordEvent)
    {
        switch (ruleObj)
        {
            case RuleDefinition<bool> boolRule:
                await ProcessTypedRuleAsync(boolRule, discordEvent);
                break;
            case RuleDefinition<string> stringRule:
                await ProcessTypedRuleAsync(stringRule, discordEvent);
                break;
        }
    }

    private async Task ProcessTypedRuleAsync<TResponse>(RuleDefinition<TResponse> rule, object discordEvent)
    {
        if (!rule.Trigger.Matches(discordEvent))
            return;

        if (await rule.Condition.EvaluateAsync(discordEvent))
        {
            var context = new EventContext
            {
                RuleId = rule.Id,
                EventType = DetermineEventType(discordEvent),
                EventData = SerializeEventData(discordEvent),
                Timestamp = DateTime.UtcNow
            };

            await _apiClient.SendEventContextAsync<TResponse>(context);
        }
    }

    /// <summary>
    /// Determines the type of the Discord.Net event object.
    /// </summary>
    /// <param name="discordEvent"></param>
    /// <returns></returns>
    private string DetermineEventType(object discordEvent)
    {
        return discordEvent switch
        {
            SocketMessage => "MessageReceived",
            SocketReaction => "ReactionAdded",
            // Add other mappings as needed
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Serializes the event data into a dictionary. Allows for layer between the event object and the API.
    /// </summary>
    /// <param name="discordEvent"></param>
    /// <returns></returns>
    private Dictionary<string, object> SerializeEventData(object discordEvent)
    {
        var data = new Dictionary<string, object>();

        switch (discordEvent)
        {
            case SocketMessage msg:
                data["channelId"] = msg.Channel.Id;
                data["content"] = msg.Content;
                data["authorId"] = msg.Author.Id;
                break;
            case SocketReaction reaction:
                data["messageId"] = reaction.MessageId;
                data["userId"] = reaction.UserId;
                data["emote"] = reaction.Emote.Name;
                break;
        }

        return data;
    }
}

