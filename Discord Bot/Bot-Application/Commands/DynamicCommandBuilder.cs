using Discord;
using Discord.Commands.Builders;
using Discord.WebSocket;
using System.Collections.Generic;

namespace Bot_Application.Commands
{
    public static class DynamicCommandBuilder
    {
        public static SlashCommandBuilder CommandBuilder { get; }

        static DynamicCommandBuilder(){
            CommandBuilder = new SlashCommandBuilder();
        }

        public static SlashCommandBuilder CreateCommand(string name, string? description = null, List<DiscordCommandOption>? options = null)
        {
            CommandBuilder.WithName(name)
                .WithDescription(description);

            if (options != null)
            {
                foreach (var option in options)
                {
                    var optionToAdd = DynamicCommandOptionBuilder.CreateOption(option.Name, option.Description, option.Type, option.Required, option.Options);
                    CommandBuilder.AddOption(optionToAdd);
                }
            }
            CommandBuilder.Build();

            return CommandBuilder;
        }

    }
}