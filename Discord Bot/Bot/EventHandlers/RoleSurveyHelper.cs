using Bot.SlashCommands.ResponseHelpers;
using Discord;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bot.EventHandlers
{
    public static class RoleSurveyHelper
    {
        public static async Task SendRoleSurvey(RoleSurvey roleSurvey, IUser user, ApplicationDbContext context)
        {
            var role = await context.Roles.FirstOrDefaultAsync(r => r.Id == roleSurvey.RoleId);
            var roleSurvey_HM = await QueryHelper.GetRoleSurvey_HM(context, roleSurvey, role);

            var content = roleSurvey_HM.MainInstance.InitialMessage;
            var embed = new EmbedBuilder().WithTitle("Role Survey").WithFields(new EmbedFieldBuilder().WithName("Id 1").WithValue(roleSurvey.Id),
                new EmbedFieldBuilder().WithName("Selected Options 1").WithValue("None")).Build();

            var buttonNames = roleSurvey_HM.Options.Select(o => o.MainInstance.Text).ToArray();
            var component = MessageComponentAndEmbedHelper.CreateButtons(3, buttonNames);
            await user.SendMessageAsync(content, embed: embed, components: component);
        }
        public static async Task SendRoleSurvey(RoleSurvey_HM roleSurvey_HM, IUser user)
        {
            var content = roleSurvey_HM.MainInstance.InitialMessage;
            var embed = new EmbedBuilder().WithTitle("Role Survey").WithFields(new EmbedFieldBuilder().WithName("Id").WithValue(roleSurvey_HM.MainInstance.Id)).Build();

            var buttonNames = roleSurvey_HM.Options.Select(o => o.MainInstance.Text).ToArray();
            var component = MessageComponentAndEmbedHelper.CreateButtons(3, buttonNames);
            await user.SendMessageAsync(content, embed: embed, components: component);
        }
        public static async Task SendNextRoleSurvey(RoleSurvey_HM roleSurvey_HM, Request request, EmbedBuilder builder)
        {
            var content = roleSurvey_HM.MainInstance.InitialMessage;

            int lastNumber = int.Parse(builder.Fields.ElementAt(builder.Fields.Count - 2).Name[3..]);
            builder.AddField(new EmbedFieldBuilder().WithName($"Id {lastNumber + 1}").WithValue(roleSurvey_HM.MainInstance.Id));
            builder.AddField(new EmbedFieldBuilder().WithName($"Selected Options {lastNumber + 1}").WithValue("None"));
            var embed = builder.Build();

            var buttonNames = roleSurvey_HM.Options.Select(o => o.MainInstance.Text).ToArray();
            var component = MessageComponentAndEmbedHelper.CreateButtons(3, buttonNames);
            await request.UpdateOriginalMessageAsync(content, component, embed);
        }


        public static int GetCurrentSurveyId(IEmbed embed)
        {
            return int.Parse(embed.Fields.ElementAt(embed.Fields.Length - 2).Value);
        }

        public static (int, int, string) GetCurrentRoleSurveyEmbedInfo(IEmbed embed)
        {
            var fieldNumber = embed.Fields.Length / 2;
            var id = int.Parse(embed.Fields.ElementAt(embed.Fields.Length - 2).Value);
            var selectedOptions = embed.Fields.ElementAt(embed.Fields.Length - 1).Value;

            return (fieldNumber, id, selectedOptions);
        }

        public static string GetSelectedOptions(IEmbed embed, int roleSurveyId)
        {
            var field = embed.Fields.FirstOrDefault(f => f.Value == roleSurveyId.ToString() && f.Name.StartsWith("Id "));
            return embed.Fields.ElementAt(embed.Fields.IndexOf(field) + 1).Value.ToString();
        }
    }
}
