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
            });
        }

        Task OnGuildJoinOrLeave(SocketGuild guild)
        {
            _ = Task.Run(async () =>
            {
                await UpdateCurrentGuilds();
                await RegisterSlashCommands();
                await UpdateVoiceChannelCurrencyGains();
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
        async Task RegisterSlashCommands()
        {
            //As I was making changes to the slash commands, I often needed to unregister and then register the commands again. So I put that on startup.
            //As number of slash commands grew, this became slow.
            //So I changed the logic to only delete a command if it is removed, or register a command if it doesn't exist.
            //Or delete and register again if properties are different.
             //There is certainly a better way to achieve this but I didn't have time.

            Console.WriteLine("Re-Registering Slash Commands (If required)...");
            foreach (var guild in client.Guilds)
            {
                var commands = await guild.GetApplicationCommandsAsync();

                foreach (var command in commands)
                {
                    //delete command if it is registered on discord but doesn't exist in slashCommands.
                    var slashCommand = slashCommands.FirstOrDefault(sc => sc.Name == command.Name);                    
                    if (slashCommand == null)
                    {
                        await SlashCommandsRegister.DeleteCommand(guild, command.Name);
                        continue;
                    }


                    //Delete and then re-register command if properties of the registered one are different from the one in slashCommands.
                    if (!ArePropertiesSame(slashCommand.Properties, command))
                    {
                        await SlashCommandsRegister.DeleteCommand(guild, command.Name);
                        await SlashCommandsRegister.RegisterCommand(guild, slashCommand.Properties);
                        Console.WriteLine($"Re-Registered {command.Name} command on guild {guild.Name}");
                    }
                }

                //add commands if they are not registered on discord but exist in slashCommands.
                foreach (var slashCommand in slashCommands)
                {
                    if(!commands.Any(c => c.Name == slashCommand.Name))
                    {
                        await SlashCommandsRegister.RegisterCommand(guild, slashCommand.Properties);
                    }                    
                }
            }
            Console.WriteLine("Registering Slash Commands Done.");
        }

        //The checks in this function only cover the part required at the time. As more complex commands are created, the proper checks need to be added here.
        static bool ArePropertiesSame(SlashCommandProperties properties, SocketApplicationCommand command)
        {
            if(properties.Description.Value != command.Description)
            {
                return false;
            }

            foreach(var option in command.Options)
            {
                var propertyOption = properties.Options.Value.FirstOrDefault(o => o.Name == option.Name);
                if(propertyOption == null)
                {
                    return false;
                }

                if(option.Description != propertyOption.Description || ((option.IsRequired ?? false) != (propertyOption.IsRequired ?? false)) || option.Type != propertyOption.Type)
                {
                    return false;
                }

                if(option.Choices.Count != propertyOption.Choices.Count)
                {
                    return false;
                }

                foreach (var choice in option.Choices)
                {
                    var propertyOptionChoice = propertyOption.Choices.FirstOrDefault(c => c.Name == choice.Name && c.Value.ToString() == choice.Value.ToString());
                    if(propertyOptionChoice == null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
