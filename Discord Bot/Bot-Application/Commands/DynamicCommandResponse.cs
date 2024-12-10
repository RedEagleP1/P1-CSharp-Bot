using Discord;
using Discord.WebSocket;

namespace Bot_Application.Commands
{
    public class DynamicCommandResponse
    {
        private string _content = string.Empty;
        private bool _isEphemeral;
        private bool _isTTS;



        public DynamicCommandResponse(string content, bool isEphemeral = false, bool isTTS = false)
        {
            _content = content;
            _isEphemeral = isEphemeral;
            _isTTS = isTTS;
        }


        public async Task RespondAsync(SocketInteraction socketInteraction)
        {
            await socketInteraction.RespondAsync(text: _content,
                                                  ephemeral: _isEphemeral,
                                                  isTTS: _isTTS);
        }
    }
}