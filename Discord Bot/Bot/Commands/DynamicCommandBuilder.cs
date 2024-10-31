using Discord;
using Discord.WebSocket;
using System.Collections.Generic;

namespace Bot.Commands
{
    public static class DynamicCommandBuilder
    {
        public static SlashCommandBuilder CreateCommand(string name, string description, List<SlashCommandOptionBuilder> options = null)
        {
            var commandBuilder = new SlashCommandBuilder()
                .WithName(name)
                .WithDescription(description);

            if (options != null)
            {
                foreach (var option in options)
                {
                    commandBuilder.AddOption(option);
                }
            }

            return commandBuilder;
        }
    }
}