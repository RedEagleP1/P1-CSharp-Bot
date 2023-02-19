using Models;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Bot.OneTimeRegister;
using Bot.SlashCommands;

namespace Bot.EventHandlers
{
    public class CurrentGuildsUpdateHandler : IEventHandler, IReadyHandler
    {
        readonly DiscordSocketClient client;
        readonly DBContextFactory dbContextFactory;
        readonly List<ISlashCommand> slashCommands;
        public CurrentGuildsUpdateHandler(DiscordSocketClient client, DBContextFactory dbContextFactory, List<ISlashCommand> slashCommands)
        {
            this.client = client;
            this.dbContextFactory = dbContextFactory;      
            this.slashCommands = slashCommands;
        }

        public void Subscribe()
        {
            client.JoinedGuild += OnGuildJoinOrLeave;
            client.LeftGuild += OnGuildJoinOrLeave;
        }

        public void OnReady()
        {
            _ = Task.Run(async () => await UpdateCurrentGuilds());
            _ = Task.Run(async () => await RegisterSlashCommands());
        }

        Task OnGuildJoinOrLeave(SocketGuild guild)
        {
            _ = Task.Run(async () => await UpdateCurrentGuilds());
            _ = Task.Run(async () => await RegisterSlashCommands());
            return Task.CompletedTask;
        }

        public async Task UpdateCurrentGuilds()
        {
            using var context = dbContextFactory.GetNewContext();
            foreach (var guild in client.Guilds)
            {
                var guildFound = await context.Guilds.FirstOrDefaultAsync(g => g.Id == guild.Id);
                if (guildFound == null)
                {
                    guildFound = new Guild()
                    {
                        Id = guild.Id,
                        Name = guild.Name,
                        IsCurrentlyJoined = true,
                        Roles = new List<Role>()
                    };

                    foreach (var role in guild.Roles)
                    {
                        guildFound.Roles.Add(new Role()
                        {
                            Id = role.Id,
                            Name = role.Name
                        });
                    }
                    context.Guilds.Add(guildFound);
                    continue;
                }

                if (!guildFound.IsCurrentlyJoined)
                    guildFound.IsCurrentlyJoined = true;
            }

            await context.SaveChangesAsync();

        }
        Task RegisterSlashCommands()
        {
            _ = Task.Run(async () =>
            {
                foreach (var guild in client.Guilds)
                {
                    var commands = await guild.GetApplicationCommandsAsync();

                    foreach (var command in commands)
                    {
                        await SlashCommandsRegister.DeleteCommand(guild, command.Name);
                    }

                    foreach (var slashCommand in slashCommands)
                    {
                        await SlashCommandsRegister.RegisterCommand(guild, slashCommand.Properties);
                    }                    
                }

            });

            return Task.CompletedTask;
        }
    }
}
