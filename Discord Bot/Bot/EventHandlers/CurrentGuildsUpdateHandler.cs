using Models;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Bot.OneTimeRegister;
using Bot.SlashCommands;
using Discord;

namespace Bot.EventHandlers
{
    public class CurrentGuildsUpdateHandler : IEventHandler, IReadyHandler
    {
        readonly DiscordSocketClient client;
        readonly List<ISlashCommand> slashCommands;
        public CurrentGuildsUpdateHandler(DiscordSocketClient client, List<ISlashCommand> slashCommands)
        {
            this.client = client;    
            this.slashCommands = slashCommands;
        }

        public void Subscribe()
        {
            client.JoinedGuild += OnGuildJoinOrLeave;
            client.LeftGuild += OnGuildJoinOrLeave;
        }

        public void OnReady()
        {
            _ = Task.Run(async () => 
            {
                await UpdateCurrentGuilds();
                await RegisterSlashCommands();
                await UpdateVoiceChannelCurrencyGains();
                await UpdateTextChannelCurrencyGainsImage();
                await UpdateTextChannelCurrencyGainsMessage();
            });
        }

        Task OnGuildJoinOrLeave(SocketGuild guild)
        {
            _ = Task.Run(async () =>
            {
                await UpdateCurrentGuilds();
                await RegisterSlashCommands();
                await UpdateVoiceChannelCurrencyGains();
                await UpdateTextChannelCurrencyGainsImage();
                await UpdateTextChannelCurrencyGainsMessage();
            });
            return Task.CompletedTask;
        }

        public async Task UpdateCurrentGuilds()
        {
            Console.WriteLine("Updating Current Guilds List in the database...");
            using var context = DBContextFactory.GetNewContext();
            foreach (var guild in client.Guilds)
            {
                var guildFound = await context.Guilds.FirstOrDefaultAsync(g => g.Id == guild.Id);
                if (guildFound == null)
                {
                    guildFound = new Guild()
                    {
                        Id = guild.Id,
                        Name = guild.Name,
                        IsCurrentlyJoined = true
                    };

                    var allRoles = new List<Role>();
                    foreach (var role in guild.Roles)
                    {
                        allRoles.Add(new Role()
                        {
                            Id = role.Id,
                            Name = role.Name,
                            GuildId = guild.Id
                        });
                    }
                    context.Guilds.Add(guildFound);
                    context.Roles.AddRange(allRoles);
                    continue;
                }

                if (!guildFound.IsCurrentlyJoined)
                    guildFound.IsCurrentlyJoined = true;
            }

            await context.SaveChangesAsync();
            Console.WriteLine("Updated Current Guilds List in the database. Done.");
        }
        public async Task UpdateVoiceChannelCurrencyGains()
        {
            Console.WriteLine("Updating Voice Channels Currency Gains List in the database...");
            var context = DBContextFactory.GetNewContext();
            foreach(var guild in client.Guilds)
            {
                foreach(var channel in guild.VoiceChannels)
                {
                    var currencyGain = await context.VoiceChannelCurrencyGains.FirstOrDefaultAsync(cg => cg.GuildId == guild.Id && cg.ChannelId == channel.Id);
                    if(currencyGain != null)
                    {
                        continue;
                    }

                    currencyGain = new VoiceChannelCurrencyGain()
                    {
                        GuildId = guild.Id,
                        ChannelId = channel.Id,
                        ChannelName = channel.Name
                    };

                    context.VoiceChannelCurrencyGains.Add(currencyGain);
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine("Updated Voice Channels Currency Gains List in the database. Done.");
        }
        public async Task UpdateTextChannelCurrencyGainsImage()
        {
            Console.WriteLine("Updating Text Channels Currency Gains For Images List in the database...");
            var context = DBContextFactory.GetNewContext();
            foreach (var guild in client.Guilds)
            {
                foreach (var channel in guild.TextChannels)
                {
                    if(channel.GetChannelType() != ChannelType.Text)
                    {
                        continue;
                    }
                    var currencyGain = await context.TextChannelsCurrencyGainImage.FirstOrDefaultAsync(cg => cg.GuildId == guild.Id && cg.ChannelId == channel.Id);
                    if (currencyGain != null)
                    {
                        continue;
                    }

                    currencyGain = new TextChannelCurrencyGainImage()
                    {
                        GuildId = guild.Id,
                        ChannelId = channel.Id,
                        ChannelName = channel.Name
                    };

                    context.TextChannelsCurrencyGainImage.Add(currencyGain);
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine("Updated Text Channels Currency Gains For Images List in the database. Done.");
        }
        public async Task UpdateTextChannelCurrencyGainsMessage()
        {
            Console.WriteLine("Updating Text Channels Currency Gains For Message List in the database...");
            var context = DBContextFactory.GetNewContext();
            foreach (var guild in client.Guilds)
            {
                foreach (var channel in guild.TextChannels)
                {
                    if (channel.GetChannelType() != ChannelType.Text)
                    {
                        continue;
                    }
                    var currencyGain = await context.TextChannelsCurrencyGainMessage.FirstOrDefaultAsync(cg => cg.GuildId == guild.Id && cg.ChannelId == channel.Id);
                    if (currencyGain != null)
                    {
                        continue;
                    }

                    currencyGain = new TextChannelCurrencyGainMessage()
                    {
                        GuildId = guild.Id,
                        ChannelId = channel.Id,
                        ChannelName = channel.Name
                    };

                    context.TextChannelsCurrencyGainMessage.Add(currencyGain);
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine("Updated Text Channels Currency Gains For Message List in the database. Done.");
        }
        async Task RegisterSlashCommands()
        {
            Console.WriteLine("Re-Registering Slash Commands...");
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
            Console.WriteLine("Registering Slash Commands Done.");
        }
    }
}
