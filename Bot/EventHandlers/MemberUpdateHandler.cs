using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;

namespace Bot.EventHandlers
{
    public class MemberUpdateHandler : IEventHandler
    {
        readonly DiscordSocketClient client;
        readonly DBContextFactory dbContextFactory;
        public MemberUpdateHandler(DiscordSocketClient client, DBContextFactory dbContextFactory)
        {
            this.client = client;
            this.dbContextFactory = dbContextFactory;
        }

        public void Subscribe()
        {
            client.GuildMemberUpdated += OnGuildMemberUpdate;
        }
        Task OnGuildMemberUpdate(Cacheable<SocketGuildUser, ulong> before, SocketGuildUser after)
        {
            _ = Task.Run(async () =>
            {
                SocketGuildUser b = await before.GetOrDownloadAsync();
                await SendRoleMessage(b, after);
            });

            return Task.CompletedTask;
        }
        async Task SendRoleMessage(SocketGuildUser before, SocketGuildUser after)
        {
            if (before.Roles.Count >= after.Roles.Count)
                return;

            using var context = dbContextFactory.GetNewContext();
            Guild? guild = await context.Guilds.AsNoTracking().Include(g => g.Roles).FirstOrDefaultAsync(g => g.Id == after.Guild.Id);
            if (guild == null)
                return;

            for (int i = 0; i < after.Roles.Count; i++)
            {
                if (i >= before.Roles.Count || before.Roles.ElementAt(i) != after.Roles.ElementAt(i))
                {
                    var role = guild.Roles.Find(role => role.Id == after.Roles.ElementAt(i).Id);
                    if (role != null && role.Message != null)
                        await after.SendMessageAsync(role.Message);

                    return;
                }
            }
        }
    }
}
