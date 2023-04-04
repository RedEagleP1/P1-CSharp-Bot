using Models;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Bot.OneTimeRegister;
using Bot.SlashCommands;
using Discord;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
            //As I was making changes to the slash commands, I often needed to unregister and then register the commands again. So I put that on startup.
            //As number of slash commands grew, this became slow.
            //So I changed the logic to only delete a command if it is removed, or register a command if it doesn't exist.
            //If you do make changes to the properties of a command, you will need to re-register them during development.
            //The part under try and catch exist because when a new currency is created, which will rarely happen, we would need to add that as an option for "award" and "currency" commands.
            //There is certainly a better way to achieve this but I didn't have time.

            Console.WriteLine("Re-Registering Slash Commands...");
            foreach (var guild in client.Guilds)
            {
                var commands = await guild.GetApplicationCommandsAsync();

                foreach (var command in commands)
                {
                    if(!slashCommands.Any(sc => sc.Name == command.Name))
                    {
                        await SlashCommandsRegister.DeleteCommand(guild, command.Name);
                    }
                }

                foreach (var slashCommand in slashCommands)
                {
                    if(!commands.Any(c => c.Name == slashCommand.Name))
                    {
                        await SlashCommandsRegister.RegisterCommand(guild, slashCommand.Properties);
                    }                    
                }

                try
                {
                    var awardCommand = commands.FirstOrDefault(c => c.Name == "award");
                    var awardSlashCommand = slashCommands.FirstOrDefault(sc => sc.Name == "award");
                    bool newCurrencyHasBeenAdded = awardSlashCommand.Properties.Options.Value.FirstOrDefault(o => o.Name == "currency").Choices.Count > awardCommand.Options.FirstOrDefault(o => o.Name == "currency").Choices.Count;
                    if (newCurrencyHasBeenAdded)
                    {
                        await SlashCommandsRegister.DeleteCommand(guild, "award");
                        await SlashCommandsRegister.DeleteCommand(guild, "currency");

                        await SlashCommandsRegister.RegisterCommand(guild, awardSlashCommand.Properties);
                        var currencySlashCommand = slashCommands.FirstOrDefault(sc => sc.Name == "currency");
                        await SlashCommandsRegister.RegisterCommand(guild, currencySlashCommand.Properties);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("There was an error while registering slash commands..");
                }

            }
            Console.WriteLine("Registering Slash Commands Done.");
        }
    }
}
