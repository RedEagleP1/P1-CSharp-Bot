using Discord;
using Discord.Commands.Builders;
using Discord.WebSocket;
using System.Collections.Generic;

namespace Bot_Application.Commands
{
    public static class DynamicCommandBuilder
    {

        static DynamicCommandBuilder(){
        }

        public static SlashCommandBuilder CreateCommand(string name, string? description = null, List<DiscordCommandOption>? options = null)
        {
            // This is instantiating a new SlashCommandBuilder per method call to improve thread safety.  
            var commandBuilder = new SlashCommandBuilder();
            commandBuilder.WithName(name)
                .WithDescription(description);

            if (options != null)
            {
                foreach (var option in options)
                {
                    var optionToAdd = DynamicCommandOptionBuilder.CreateOption(option.Name, option.Type, option.Description, option.Required, option.Options);
                    commandBuilder.AddOption(optionToAdd);
                }
            }
            return commandBuilder;
        }

    }
}