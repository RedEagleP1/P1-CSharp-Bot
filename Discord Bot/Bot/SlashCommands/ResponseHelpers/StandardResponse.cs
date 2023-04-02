using Discord;
using Discord.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bot.SlashCommands.ResponseHelpers
{
    public class StandardResponse : IResponse
    {
        string content;
        MessageComponent component;

        bool hasFieldToAdd;
        string fieldToAdd;
        Conditions triggerConditions;

        bool ensureFormat;
        Regex format;

        public StandardResponse WithContent(string content)
        {
            this.content = content;
            return this;
        }

        public StandardResponse WithButtons(params string[] buttonNames)
        {
            component = MessageComponentAndEmbedHelper.CreateButtons(buttonNames);
            return this;
        }

        public StandardResponse WithFieldToAdd(string fieldName)
        {
            hasFieldToAdd = true;
            fieldToAdd = fieldName;
            return this;
        }

        public StandardResponse EnsureFormat(Regex format)
        {
            ensureFormat = true;
            this.format = format;
            return this;
        }

        public StandardResponse WithConditions(Conditions conditions)
        {
            triggerConditions = conditions;
            return this;
        }

        public async Task HandleResponse(Request request)
        {
            if(ensureFormat && !format.IsMatch(request.IncomingValue))
            {
                await request.RespondSeparatelyAsync("Looks like the format is wrong, try again.", ephemeral: true);
                return;
            }

            Embed embed = null;

            if(hasFieldToAdd)
            {
                embed = request.HasEmbedField(fieldToAdd, out _)
                ? MessageComponentAndEmbedHelper.ChangeField(request.Embed, fieldToAdd, request.IncomingValue)
                : MessageComponentAndEmbedHelper.AddField(request.Embed, fieldToAdd, request.IncomingValue);
            }    
            
            if(embed == null)
            {
                embed = request.Embed.ToEmbedBuilder().Build();
            }

            await request.UpdateOriginalMessageAsync(content, component, embed);
        }

        public bool ShouldRespond(Request request)
        {
            return triggerConditions.CheckConditions(request);
        }
    }
}
