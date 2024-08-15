using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
using static System.Collections.Specialized.BitVector32;
using System.Reflection.Metadata;
using Bot.EventHandlers;

namespace Bot.SlashCommands.ResponseHelpers
{
    public class Request
    {
        SocketModal modal;
        SocketMessageComponent component;
        SocketSlashCommand command;
        public ComponentType IncomingComponentType { get; private set; }
        public string IncomingModalName { get; private set; } = null;
        public string IncomingValue { get; private set; } = null;
        public IMessage Message { get; private set; } = null;
        public SocketUser User { get; private set; } = null;
        public IEmbed? Embed { get; private set; }
        public string TaskType { get; private set; } = string.Empty;
        public IMessage ReferencedMessage { get; set; } = null;

        public ulong? GuildId { get; set; }

        public Request(SocketModal modal)
        {
            this.modal = modal;
            IncomingComponentType = ComponentType.Modal;
        }

        public Request(SocketMessageComponent component)
        {
            this.component = component;
            IncomingComponentType = ComponentType.MessageComponent;
        }

        public Request(SocketSlashCommand command)
        {
            this.command = command;
            IncomingComponentType = ComponentType.SlashCommand;
        }

        async Task ExtractValues()
        {
            switch (IncomingComponentType)
            {
                case ComponentType.MessageComponent:
                    Message = component.Message;
                    IncomingValue = component.Data.CustomId;
                    User = component.User;
                    GuildId = component.GuildId;
                    Embed = Message.Embeds.Count > 0 ? Message.Embeds.First() : null;
                    if (Message.Reference != null)
                    {
                        if(RequestInterceptor.IsForAccount(Embed.Title))
                        {
                            ReferencedMessage = await DiscordQueryHelper.AccountPostChannel.GetMessageAsync(Message.Reference.MessageId.Value);
                            break;
                        }
                        if(RequestInterceptor.IsForReview(Embed.Title))
                        {
                            ReferencedMessage = await DiscordQueryHelper.ReviewPostChannel.GetMessageAsync(Message.Reference.MessageId.Value);
                        }                        
                    }
                    break;
                case ComponentType.Modal:
                    var restInteractionMessage = await modal.GetOriginalResponseAsync();
                    Message = restInteractionMessage;
                    IncomingModalName = modal.Data.Components.First().CustomId;
                    IncomingValue = modal.Data.Components.First().Value;
                    User = modal.User;
                    GuildId = modal.GuildId;
                    Embed = Message.Embeds.Count > 0 ? Message.Embeds.First() : null;
                    if (Message.Reference != null)
                    {
                        if (RequestInterceptor.IsForAccount(Embed.Title))
                        {
                            ReferencedMessage = await DiscordQueryHelper.AccountPostChannel.GetMessageAsync(Message.Reference.MessageId.Value);
                            break;
                        }
                        if (RequestInterceptor.IsForReview(Embed.Title))
                        {
                            ReferencedMessage = await DiscordQueryHelper.ReviewPostChannel.GetMessageAsync(Message.Reference.MessageId.Value);
                        }
                    }
                    break;
            }
        }

        public async Task ProcessRequest()
        {
            await ExtractValues();
        }

        public async Task SendModalAsync(string modalName, bool shortInput = false, string placeHolder = "Type Here..")
        {
            if (IncomingComponentType != ComponentType.MessageComponent)
            {
                return;
            }

            TextInputStyle style = TextInputStyle.Paragraph;
            int maxCharacters = 1000;

            if (shortInput)
            {
                style = TextInputStyle.Short;
                maxCharacters = 50;
            }

            var modal = new ModalBuilder()
                            .WithTitle(modalName)
                            .WithCustomId(modalName)
                            .AddTextInput(modalName, modalName, placeholder: placeHolder,
                            style: style, maxLength: maxCharacters, required: true).Build();

            await component.RespondWithModalAsync(modal);
        }

        public async Task UpdateOriginalMessageAsync(string content, MessageComponent messageComponent, Embed embed)
        {
            Action<MessageProperties> action = (messageProperties) =>
            {
                messageProperties.Content = content;
                messageProperties.Components = messageComponent ?? new ComponentBuilder().Build();
                if (embed == null)
                {
                    messageProperties.Flags = MessageFlags.SuppressEmbeds;
                    return;
                }
                messageProperties.Embed = embed;
            };

            switch (IncomingComponentType)
            {
                case ComponentType.MessageComponent:
                    await component.ModifyOriginalResponseAsync(action);
                    break;
                case ComponentType.Modal:
                    await modal.ModifyOriginalResponseAsync(action);
                    break;
                case ComponentType.SlashCommand:
                    await command.ModifyOriginalResponseAsync(action);
                    break;
            }
        }

        public async Task RespondSeparatelyAsync(string content, MessageComponent components = null, Embed embed = null, bool ephemeral = false)
        {
            switch (IncomingComponentType)
            {
                case ComponentType.MessageComponent:
                    if (component.HasResponded)
                    {
                        await component.FollowupAsync(content, components: components, embed: embed, ephemeral: ephemeral);
                        break;
                    }
                    await component.RespondAsync(content, components: components, embed: embed, ephemeral: ephemeral);
                    break;
                case ComponentType.Modal:
                    await modal.FollowupAsync(content, components: components, embed: embed, ephemeral: ephemeral);
                    break;
            }
        }

        public async Task DeleteOriginalMessageAsync()
        {
            switch (IncomingComponentType)
            {
                case ComponentType.MessageComponent:
                    await component.DeleteOriginalResponseAsync();
                    break;
                case ComponentType.Modal:
                    await modal.DeleteOriginalResponseAsync();
                    break;
            }
        }

        public async Task DeleteReferencedMessageAsync()
        {
            await ReferencedMessage.DeleteAsync();
        }

        public bool HasEmbedField(string fieldName, out string value)
        {
            foreach (var field in Embed.Fields)
            {
                if (field.Name == fieldName)
                {
                    value = field.Value;
                    return true;
                }
            }

            value = string.Empty;
            return false;
        }
    }

    public enum ComponentType
    {
        MessageComponent,
        Modal,
        SlashCommand
    }
}
