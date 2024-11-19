using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Bot.Commands
{
    // CAP
    // MF 

    public static class DynamicCommandResponseBuilder
    {
        public static DynamicCommandResponse CreateResponse(SocketInteraction socketInteraction, string title, string description, string content = "", Color? color = null, bool isEphemeral = false, bool isTTS = false)
        {
            EmbedBuilder embedBuilder = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description);

            if (color != null)
                embedBuilder.WithColor((Color)color);

            return new DynamicCommandResponse(socketInteraction, embedBuilder, content, isEphemeral, isTTS);
        }
    }
}
