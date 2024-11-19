using Discord;

namespace Bot.Commands
{
    public static class DynamicCommandOptionBuilder
    {
        public static SlashCommandOptionBuilder CreateOption(string name, string description, ApplicationCommandOptionType type, bool required = false, List<DiscordCommandOption> options = null)
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
                    var optionToAdd = CreateOption(option.Name, option.Description, option.Type, option.Required, option.Options);
                    optionBuilder.AddOption(optionToAdd);
                }
            }

            return optionBuilder;
        }
    }
}