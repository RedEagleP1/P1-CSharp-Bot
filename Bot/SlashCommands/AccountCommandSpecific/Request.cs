using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace Bot.SlashCommands.AccountCommandSpecific
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
        public IReadOnlyCollection<SocketUser> MessageUserMentions { get; private set; } = null;
        public IEmbed? Embed { get; private set; }
        public string TaskType { get; private set; } = string.Empty;
        public MessageType MessageType { get; private set; }

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
                    MessageUserMentions = component.Message.MentionedUsers;
                    break;
                case ComponentType.Modal:
                    Message = await modal.GetOriginalResponseAsync();
                    IncomingModalName = modal.Data.Components.First().CustomId;
                    IncomingValue = modal.Data.Components.First().Value;
                    User = modal.User;
                    break;
            }
        }

        async Task AnalyzeMessage()
        {
            Embed = Message.Embeds.Count > 0 ? Message.Embeds.First() : null;
            CheckMessageType(IncomingValue);
        }
        void CheckMessageType(string incomingValue)
        {
            MessageType = MessageType.None;

            if (Embed == null)
            {
                return;
            }

            switch (Embed.Title)
            {
                case HelperStrings.accountYourHours:
                    MessageType = MessageType.TaskInfoCreationMessage;                    
                    if(Embed.Fields.FirstOrDefault(field => field.Name == HelperStrings.currency).Value != null)
                    {
                        var taskType = Embed.Fields.FirstOrDefault(field => field.Name == HelperStrings.taskType).Value;
                        TaskType = taskType ?? incomingValue;
                    }
                    break;
                case HelperStrings.verificationRequired:
                    MessageType = MessageType.VerificationRequestMessage;
                    TaskType = Embed.Fields.FirstOrDefault(field => field.Name == HelperStrings.taskType).Value;
                    break;
                case HelperStrings.verification:
                    MessageType = MessageType.VerificationProcessMessage;
                    break;
            }
        }

        public async Task ProcessRequest()
        {
            await ExtractValues();
            if(IncomingComponentType == ComponentType.SlashCommand)
            {
                return;
            }

            await AnalyzeMessage();
        }

        public async Task SendModalAsync(string modalName, bool shortInput = false, string placeHolder = "Type Here..")
        {
            if(IncomingComponentType != ComponentType.MessageComponent)
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
                    await component.UpdateAsync(action);
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

        public bool HasEmbedField(string fieldName, out string value)
        {
            foreach(var field in Embed.Fields)
            {
                if(field.Name == fieldName)
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
