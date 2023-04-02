using Discord.WebSocket;
using Models;
using Microsoft.EntityFrameworkCore;

namespace Bot.EventHandlers
{
    public  class GuildRolesChangeHandler : IEventHandler
    {
        readonly DiscordSocketClient client;
        public GuildRolesChangeHandler(DiscordSocketClient client)
        {
            this.client = client;           
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
                using var context = DBContextFactory.GetNewContext();
                var role = await context.Roles.FirstOrDefaultAsync(r => r.Id == before.Id);
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
                using var context = DBContextFactory.GetNewContext();
                context.Roles.Add(new Role()
                {
                    Id = role.Id,
                    Name = role.Name,
                    GuildId = role.Guild.Id
                });

                await context.SaveChangesAsync();
            });

            return Task.CompletedTask;            
        }
        private Task OnRoleDeleted(SocketRole role)
        {
            _ = Task.Run(async () =>
            {
                using var context = DBContextFactory.GetNewContext();
                var roleToRemove = await context.Roles.FirstOrDefaultAsync(r => r.Id == role.Id);
                if(roleToRemove == null)
                {
                    return;
                }

                context.Roles.Remove(roleToRemove);
                await context.SaveChangesAsync();
            });

            return Task.CompletedTask;            
        }
    }
}
