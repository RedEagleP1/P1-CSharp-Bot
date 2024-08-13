using Bot.SlashCommands;
using Bot.SlashCommands.ResponseHelpers;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.EventHandlers
{
    public class RequestInterceptor
    {
        public string TargetCommand { get; private set; }
        public bool IsRoleSurveyInteraction { get; private set; }
        public Request Request { get; private set; }

        public RequestInterceptor(SocketMessageComponent component)
        {
            Request = new Request(component);            
        }

        public RequestInterceptor(SocketModal modal)
        {
            Request = new Request(modal);
        }

        public async Task Process()
        {
            await Request.ProcessRequest();

            if (Request.Embed == null)
                return;

            var embedTitle = Request.Embed.Title;
            if(IsForAccount(embedTitle))
            {
                TargetCommand = "account";
                return;
            }

            if (IsForReview(embedTitle))
            {
                TargetCommand = "review";
                return;
            }

            if(embedTitle == "Roles For Sale")
            {
                TargetCommand = "buyrole";
                return;
            }

            if(embedTitle == "Role Survey")
            {
                IsRoleSurveyInteraction = true;
            }
        }

        public static bool IsForReview(string embedTitle)
        {
            return embedTitle == "Review" || embedTitle == "Review (Verification)" || embedTitle == "Review (Verification Process)";
        }

        public static bool IsForAccount(string embedTitle)
        {
            return embedTitle == "Account" || embedTitle == "Account (Verification)" || embedTitle == "Account (Verification Process)" || embedTitle == "Verification Required";
        }
    }
}
