using Discord;
using Discord.WebSocket;
using DiscordBot.Models;
using DiscordBot.Data;
using Microsoft.EntityFrameworkCore;

public class MemberRoleEvent
{
    DiscordSocketClient client;
    IServiceProvider _sp;
    public MemberRoleEvent(DiscordSocketClient client, IServiceProvider sp)
    {
        this.client = client;
        _sp = sp;
        client.GuildMemberUpdated += OnGuildMemberUpdate;
    }

    async Task OnGuildMemberUpdate(Cacheable<SocketGuildUser, ulong> before, SocketGuildUser after)
    {
        SocketGuildUser b = await before.GetOrDownloadAsync();
        await CheckRoleChange(b, after);
    }

    async Task CheckRoleChange(SocketGuildUser before, SocketGuildUser after)
    {
        if (before.Roles.Count >= after.Roles.Count)
            return;

        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Guild? guild = db.Guilds.AsNoTracking().Include(g => g.RoleMessages).FirstOrDefault(g => g.ID == after.Guild.Id);
        if (guild == null)
            return;

        for (int i = 0; i < after.Roles.Count; i++)
        {
            if (i >= before.Roles.Count || before.Roles.ElementAt(i) != after.Roles.ElementAt(i))
            {
                foreach(var r in guild.RoleMessages)
                {
                    if(r.RoleID == after.Roles.ElementAt(i).Id)
                    {
                        await after.SendMessageAsync(r.Message);
                        break;
                    }
                }

                return;
            }
        }
    }
}