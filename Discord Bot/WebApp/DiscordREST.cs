using Discord.Rest;
using Models.HelperClasses;
using Discord;
public static class DiscordREST
{
    public static DiscordRestClient discordRestClient;
    public static async Task Init(string botToken)
    {
        discordRestClient = new DiscordRestClient();
        await discordRestClient.LoginAsync(TokenType.Bot, botToken);
    }

    public static async Task<IEnumerable<TextChannel>> GetTextChannelsAsync(ulong guildId)
    {
        var textChannels = new List<TextChannel>();
        var guild = await discordRestClient.GetGuildAsync(guildId);
        if(guild == null)
        {
            return textChannels;
        }
        var restChannels = await guild.GetChannelsAsync();
        foreach(var channel in restChannels)
        {
            if(channel is not ITextChannel)
            {
                continue;
            }

            textChannels.Add(new TextChannel()
            {
                GuildId = guildId,
                ChannelId = channel.Id,
                ChannelName = channel.Name
            });
        }

        return textChannels;
    }
    public static async Task<TextChannel?> GetTextChannel(ulong guildId, ulong channelId)
    {
        var guild = await discordRestClient.GetGuildAsync(guildId);
        if(guild == null)
        {
            return null;
        }
        var channel = await guild.GetChannelAsync(channelId);
        if(channel == null)
        {
            return null;
        }
        return new TextChannel()
        {
            GuildId = guildId,
            ChannelId = channelId,
            ChannelName = channel.Name
        };
    }
}
