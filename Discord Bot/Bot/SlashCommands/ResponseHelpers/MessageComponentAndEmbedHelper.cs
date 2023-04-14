using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.SlashCommands.ResponseHelpers
{
    public static class MessageComponentAndEmbedHelper
    {
        public static MessageComponent CreateButtons(params string[] buttonNames)
        {
            var componentBuilder = new ComponentBuilder();
            for (int i = 0; i < buttonNames.Length; i++)
            {               
                componentBuilder.WithButton(buttonNames[i], buttonNames[i], row: i%4);
            }
            return componentBuilder.Build();
        }
        public static MessageComponent CreateButtons(int maxRows, params string[] buttonNames)
        {
            maxRows = Math.Clamp(maxRows, 0, 4);
            var componentBuilder = new ComponentBuilder();
            for (int i = 0; i < buttonNames.Length; i++)
            {
                componentBuilder.WithButton(buttonNames[i], buttonNames[i], row: i % maxRows);
            }
            return componentBuilder.Build();
        }
        public static MessageComponent CreateButtons(string specialButton, params string[] buttonNames)
        {
            var componentBuilder = new ComponentBuilder();
            for (int i = 0; i < buttonNames.Length; i++)
            {
                componentBuilder.WithButton(buttonNames[i], buttonNames[i], row: i % 3);
            }

            componentBuilder.WithButton(specialButton, specialButton, style: ButtonStyle.Success, row: 4, emote: Emoji.Parse(":white_check_mark:"));
            return componentBuilder.Build();
        }

        public static void ChangeField(EmbedBuilder embedBuilder, string fieldName, string newValue)
        {
            var field = embedBuilder.Fields.FirstOrDefault(field => field.Name == fieldName);
            if (field == null)
            {
                return;
            }

            field.Value = newValue;
        }

        public static Embed ChangeField(IEmbed embed, string fieldName, string newValue)
        {
            var embedBuilder = embed.ToEmbedBuilder();
            ChangeField(embedBuilder, fieldName, newValue);
            return embedBuilder.Build();
        }

        public static void AddField(EmbedBuilder embedBuilder, string fieldName, string fieldValue)
        {
            embedBuilder.AddField(new EmbedFieldBuilder().WithName(fieldName).WithValue(fieldValue));
        }

        public static Embed AddField(IEmbed embed, string fieldName, string fieldValue)
        {
            var embedBuilder = embed.ToEmbedBuilder();
            AddField(embedBuilder, fieldName, fieldValue);
            return embedBuilder.Build();
        }

        public static EmbedFieldBuilder? GetField(EmbedBuilder builder, string fieldName)
        {
            return builder.Fields.FirstOrDefault(field => field.Name == fieldName);
        }
    }
}
