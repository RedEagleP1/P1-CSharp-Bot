using Discord;
using Discord.WebSocket;
using DiscordBot.Models;

namespace DiscordBot.Bot
{
    public class DiscordBotService : IHostedService
    {
        DiscordSocketClient client;
        private readonly string botToken;
        IServiceProvider _sp;
        public DiscordBotService(string botToken, IServiceProvider sp)
        {
            this.botToken = botToken;
            _sp = sp;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await BotMain();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        async Task BotMain()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All
            });
            client.Log += LogMessage;

            await client.LoginAsync(TokenType.Bot, botToken);
            await client.StartAsync();
            client.Ready += SubscribeToEvents;
        }
        Task LogMessage(LogMessage logMessage)
        {
            Console.WriteLine(logMessage);
            return Task.CompletedTask;
        }

        Task SubscribeToEvents()
        {
            new MemberRoleEvent(client, _sp);
            return Task.CompletedTask;
        }

        public List<Guild> GetGuilds()
        {
            List<Guild> guilds = new List<Guild>();
            foreach(var g in client.Guilds)
            {
                var guild = new Guild()
                {
                    ID = g.Id,
                    Name = g.Name,
                    RoleMessages = new List<RoleMessage>()
                };
                guilds.Add(guild);
            }
            return guilds;
        }

        public Dictionary<ulong, string> GetRoles(ulong guildID)
        {
            Dictionary<ulong, string> roles = new Dictionary<ulong, string>();
            foreach(var role in client.GetGuild(guildID).Roles)
            {
                roles.Add(role.Id, role.Name);
            }
            return roles;
        }
    } 
}
