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
    static class ModalLauncher
    {
        static Dictionary<string, Modal> allModals = new();

        public static void AddModal(string buttonName, string modalName, bool shortInput = false, string placeHolder = "Type Here..")
        {
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

            allModals[buttonName] = modal;
        }

        public static async Task<bool> LaunchModalIfNeeded(SocketMessageComponent component)
        {
            if(!allModals.ContainsKey(component.Data.CustomId))
            {
                return false;
            }

            await component.RespondWithModalAsync(allModals[component.Data.CustomId]);
            return true;
        }
    }
}
