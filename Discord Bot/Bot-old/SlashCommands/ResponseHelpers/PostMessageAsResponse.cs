using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.SlashCommands.ResponseHelpers
{
    public class PostMessageAsResponse : IResponse
    {
        string content;
        public string fieldToAdd;
        Conditions triggerConditions;
        SocketTextChannel postChannel;

        Func<Request, string> postMessageContentCreation;
        MessageComponent postMessageComponent;

        bool changeEmbedTitle;
        string newEmbedTitle;

        List<string> fieldsToEncrypt = new();

        List<KeyValuePair<string, string>> fieldsToAdd = new();

        public PostMessageAsResponse WithContent(string content)
        {
            this.content = content;
            return this;
        }

        public PostMessageAsResponse WithFieldToAdd(string fieldName)
        {
            fieldToAdd = fieldName;
            return this;
        }

        public PostMessageAsResponse OnChannel(SocketTextChannel channel)
        {
            postChannel = channel;
            return this;
        }

        public PostMessageAsResponse WithPostMessageContent(Func<Request, string> contentCreation)
        {
            postMessageContentCreation = contentCreation;
            return this;
        }

        public PostMessageAsResponse WithPostMessageButtons(params string[] buttonNames)
        {
            postMessageComponent = MessageComponentAndEmbedHelper.CreateButtons(buttonNames);
            return this;
        }

        public PostMessageAsResponse WithPostMessageNewEmbedTitle(string newTitle)
        {
            changeEmbedTitle = true;
            newEmbedTitle = newTitle;
            return this;
        }

        public PostMessageAsResponse WithEncryptedFields(params string[] fieldNames)
        {
            fieldsToEncrypt = fieldNames.ToList();
            return this;
        }

        public PostMessageAsResponse WithPostMessageAddFields(params KeyValuePair<string, string>[] fieldsToAdd)
        {
            this.fieldsToAdd = fieldsToAdd.ToList();
            return this;
        }

        public PostMessageAsResponse WithConditions(Conditions conditions)
        {
            triggerConditions = conditions;
            return this;
        }

        public async Task HandleResponse(Request request)
        {
            var embed = request.HasEmbedField(fieldToAdd, out _)
                ? MessageComponentAndEmbedHelper.ChangeField(request.Embed, fieldToAdd, request.IncomingValue)
                : MessageComponentAndEmbedHelper.AddField(request.Embed, fieldToAdd, request.IncomingValue);

            await request.UpdateOriginalMessageAsync(content, null, embed);

            await PostMessage(request, embed);
        }

        public bool ShouldRespond(Request request)
        {
            return triggerConditions.CheckConditions(request);
        }

        async Task PostMessage(Request request, Embed embed)
        {
            if (postChannel == null)
            {
                return;
            }

            string content = postMessageContentCreation.Invoke(request);
            var embedTitle = changeEmbedTitle ? newEmbedTitle : request.Embed.Title;
            var builder = new EmbedBuilder().WithTitle(embedTitle);

            foreach (var field in embed.Fields)
            {
                builder.AddField(field.Name, field.Value);
            }

            foreach(var field in fieldsToAdd)
            {
                builder.AddField(field.Key, field.Value);
            } 

            foreach(var field in fieldsToEncrypt)
            {
                var fieldToEncrypt = builder.Fields.FirstOrDefault(f => f.Name == field);
                fieldToEncrypt.Value = Encryptor.Encrypt(fieldToEncrypt.Value.ToString());
            }

            await postChannel.SendMessageAsync(content, components: postMessageComponent, embed: builder.Build());
        }
    }
}
