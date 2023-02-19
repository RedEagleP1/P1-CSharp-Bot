using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.SlashCommands.AccountCommandSpecific
{
    public static class EmbedHelper
    {
        public static void ChangeField(EmbedBuilder embedBuilder, string fieldName, string newValue)
        {
            var field = embedBuilder.Fields.FirstOrDefault(field => field.Name == fieldName);
            if(field == null)
            {
                return;
            }

            field.Value = newValue;
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
