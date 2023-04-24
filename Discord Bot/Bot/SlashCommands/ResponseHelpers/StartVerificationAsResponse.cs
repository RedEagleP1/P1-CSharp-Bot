using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bot.SlashCommands.ResponseHelpers
{
    public class StartVerificationAsResponse : IResponse
    {
        string content;
        Embed embed;
        MessageComponent component;
        Conditions triggerConditions;

        public StartVerificationAsResponse WithContent(string content)
        {
            this.content = content;
            return this;
        }

        public StartVerificationAsResponse WithEmbedTitle(string title)
        {
            embed = new EmbedBuilder().WithTitle(title).Build();
            return this;
        }

        public StartVerificationAsResponse WithButtons(params string[] buttonNames)
        {
            component = MessageComponentAndEmbedHelper.CreateButtons(buttonNames);
            return this;
        }

        public StartVerificationAsResponse WithConditions(Conditions conditions)
        {
            triggerConditions = conditions;
            return this;
        }

        public async Task HandleResponse(Request request)
        {
            if (request.User.Id == FormatHelper.ExtractUserMentionsIDs(request.Message.Content).FirstOrDefault())
            {
                await request.RespondSeparatelyAsync("You can't verify your own work. Ask someone else to verify it.", ephemeral: true);
                return;
            }

            if (HasVerified(request.User.Id, request))
            {
                await request.RespondSeparatelyAsync("Looks like you have already verified once. You can't verify again. Let someone else verify it.", ephemeral: true);
                return;
            }

            await request.RespondSeparatelyAsync(content, component, embed, true);
        }

        public bool ShouldRespond(Request request)
        {
            return triggerConditions.CheckConditions(request);
        }

        static bool HasVerified(ulong userId, Request request)
        {
            var verifiers = request.Message.Embeds.First().Fields.FirstOrDefault(field => field.Name == HelperStrings.verifiers).Value;
            var matchCollection = Regex.Matches(verifiers, "<(?:@|@!)(\\d+)>");
            foreach (Match match in matchCollection)
            {
                if (ulong.Parse(match.Groups[1].Value) == userId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
