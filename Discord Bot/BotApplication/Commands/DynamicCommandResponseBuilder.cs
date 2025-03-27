using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace DiscordBot.BotApplication.Commands
{
    public static class DynamicCommandResponseBuilder
    {
        public static DynamicCommandResponse CreateResponse(string title, string description, string content = "", Color? color = null, bool isEphemeral = false, bool isTTS = false)
        {
            return new DynamicCommandResponse(content, isEphemeral, isTTS);
        }
    }
}
