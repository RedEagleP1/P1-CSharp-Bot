using Discord.WebSocket;
using Models;
using Microsoft.EntityFrameworkCore;

namespace Bot.EventHandlers
{
    public  class GuildRolesChangeHandler : IEventHandler
    {
        readonly DiscordSocketClient client;
        readonly DBContextFactory dbContextFactory;
        public GuildRolesChangeHandler(DiscordSocketClient client, DBContextFactory dbContextFactory)
        {
            this.client = client;
            this.dbContextFactory = dbContextFactory;            
        }

        public void Subscribe()
        {
            client.RoleCreated += OnRoleCreated;
            client.RoleDeleted += OnRoleDeleted;
            client.RoleUpdated += OnRoleUpdated;
        }
        private Task OnRoleUpdated(SocketRole before, SocketRole after)
        {
            if (before.Name == after.Name)
                return Task.CompletedTask;

            _ = Task.Run(async () =>
            {
                using var context = dbContextFactory.GetNewContext();
                var guild = await context.Guilds.Include(g => g.Roles).FirstOrDefaultAsync(g => g.Id == before.Guild.Id);
                if (guild == null)
                    return;

                var role = guild.Roles.Find(role => role.Id == before.Id);
                if (role != null)
                    role.Name = after.Name;

                await context.SaveChangesAsync();
            });

            return Task.CompletedTask;
        }

        private Task OnRoleCreated(SocketRole role)
        {
            _ = Task.Run(async () =>
            {
                using var context = dbContextFactory.GetNewContext();
                var guild = await context.Guilds.Include(g => g.Roles).FirstOrDefaultAsync(g => g.Id == role.Guild.Id);
                if (guild == null)
                    return;

                guild.Roles.Add(new Role()
                {
                    Id = role.Id,
                    Name = role.Name
                });

                await context.SaveChangesAsync();
            });

            return Task.CompletedTask;            
        }
        private Task OnRoleDeleted(SocketRole role)
        {
            _ = Task.Run(async () =>
            {
                using var context = dbContextFactory.GetNewContext();
                var guild = await context.Guilds.Include(g => g.Roles).FirstOrDefaultAsync(g => g.Id == role.Guild.Id);
                if (guild == null)
                    return;

                for (int i = 0; i < guild.Roles.Count; i++)
                {
                    if (guild.Roles[i].Id == role.Id)
                    {
                        guild.Roles.RemoveAt(i);
                        break;
                    }
                }

                await context.SaveChangesAsync();
            });

            return Task.CompletedTask;            
        }
    }
}
