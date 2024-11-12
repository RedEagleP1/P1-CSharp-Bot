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


        /*
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

        public DynamicCommandResponseBuilder WithTitle(string title)Bot/Commands/DynamicCommandResponseBuilder.cs
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

        */
    }
}


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