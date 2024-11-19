using Discord;
using Discord.Commands.Builders;
using Discord.WebSocket;
using System.Collections.Generic;

namespace Bot.Commands
{
    public static class DynamicCommandBuilder
    {
        public static SlashCommandBuilder CommandBuilder { get; set; }

        static DynamicCommandBuilder(){
            CommandBuilder = new SlashCommandBuilder();
        }

        public static SlashCommandBuilder CreateCommand(string name, string description, List<DiscordCommandOption> options = null)
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

            return CommandBuilder;
        }

    }
}