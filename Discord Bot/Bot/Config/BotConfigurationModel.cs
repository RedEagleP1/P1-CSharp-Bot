namespace Bot
{
    public class BotConfigurationModel
    {
        public string Token { get; set; }
        public string Prefix { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string GuildId { get; set; }
        public string CommandEndpoint { get; set; }
        public string DiscordApiUrl { get; internal set; }
    }
}