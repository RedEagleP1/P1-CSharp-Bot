using Discord;

namespace Bot.Commands
{
    public static class DynamicCommandOptionBuilder
    {
        public static SlashCommandOptionBuilder CreateOption(string name, string description, ApplicationCommandOptionType type, bool required = false, List<SlashCommandOptionBuilder> options = null)
        {
            var optionBuilder = new SlashCommandOptionBuilder()
                .WithName(name)
                .WithDescription(description)
                .WithType(type)
                .WithRequired(required);

            if (options != null)
            {
                foreach (var option in options)
                {
                    optionBuilder.AddOption(option);
                }
            }

            return optionBuilder;
        }
    }
}