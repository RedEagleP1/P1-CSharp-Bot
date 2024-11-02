using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Bot.Commands
{
    // TODO This needs to be written as a static factory class similar to the dynamic builders
    // CAP
    // MF 

    public class DynamicCommandResponseBuilder
    {
        private readonly SocketInteraction _interaction;
        private readonly EmbedBuilder _embedBuilder;
        private bool _isEphemeral;
        private string _content;
        private bool _isTTS;

        public DynamicCommandResponseBuilder(SocketInteraction interaction)
        {
            _interaction = interaction;
            _embedBuilder = new EmbedBuilder();
            _isEphemeral = true; // Default to ephemeral
            _content = string.Empty;
            _isTTS = false;
        }

        public DynamicCommandResponseBuilder WithTitle(string title)
        {
            _embedBuilder.WithTitle(title);
            return this;
        }

        public DynamicCommandResponseBuilder WithDescription(string description)
        {
            _embedBuilder.WithDescription(description);
            return this;
        }

        public DynamicCommandResponseBuilder WithColor(Color color)
        {
            _embedBuilder.WithColor(color);
            return this;
        }

        public DynamicCommandResponseBuilder WithEphemeral(bool isEphemeral)
        {
            _isEphemeral = isEphemeral;
            return this;
        }

        public DynamicCommandResponseBuilder WithContent(string content)
        {
            _content = content;
            return this;
        }

        public DynamicCommandResponseBuilder WithTTS(bool isTTS)
        {
            _isTTS = isTTS;
            return this;
        }

        public async Task RespondAsync()
        {
            var embed = _embedBuilder.Build();

            await _interaction.RespondAsync(text: _content, embed: embed, ephemeral: _isEphemeral, isTTS: _isTTS);
        }
    }
}