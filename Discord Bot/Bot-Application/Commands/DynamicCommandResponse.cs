using Discord;
using Discord.WebSocket;

namespace Bot_Application.Commands
{
    public class DynamicCommandResponse
    {
        private SocketInteraction _socketInteraction;
        private EmbedBuilder _embedBuilder;
        private string _content = string.Empty;
        private bool _isEphemeral;
        private bool _isTTS;



        public DynamicCommandResponse(SocketInteraction socketInteraction, EmbedBuilder embedBuilder, string content = "", bool isEphemeral = false, bool isTTS = false)
        {
            _socketInteraction = socketInteraction;
            _embedBuilder = embedBuilder;
            _content = content;
            _isEphemeral = isEphemeral;
            _isTTS = isTTS;
        }


        public async Task RespondAsync()
        {
            var embed = _embedBuilder.Build();

            await _socketInteraction.RespondAsync(text: _content,
                                                  embed: embed,
                                                  ephemeral: _isEphemeral,
                                                  isTTS: _isTTS);
        }
    }
}