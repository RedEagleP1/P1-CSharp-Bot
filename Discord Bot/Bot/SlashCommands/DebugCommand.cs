using Bot.SlashCommands.ResponseHelpers;
using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;
using Bot.PeriodicEvents;

namespace Bot.SlashCommands
{
    internal class DebugCommand : IRespondToButtonsAndModals, INeedAwake
    {
        const string name = "debug";
        readonly SlashCommandProperties properties = CreateNewProperties();
        readonly DiscordSocketClient client;
        public string Name => name;
        public SlashCommandProperties Properties => properties;
        public DebugCommand(DiscordSocketClient client)
        {
            this.client = client;
        }
        public void Awake()
        {
            var guild = client.GetGuild(Settings.P1OCGuildId);
            if (guild == null)
            {
                return;
            }
            var postChannel = guild.GetTextChannel(Settings.ReviewChannelId);
            if (postChannel == null)
            {
                return;
            }

            DiscordQueryHelper.SetReviewPostChannel(postChannel);
        }
        public async Task HandleCommand(SocketSlashCommand command)
        {
            if (command.Data.Name != "debug")
            {
                return;
            }

            await command.DeferAsync(ephemeral: true);

            _ = Task.Run(async () =>
            {
                try
                {
                    await command.ModifyOriginalResponseAsync((mp) =>
                    {
                        TimerCST.ResetCurrencyLogic();
                        mp.Content = "Debug Command Run.";
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            });
        }

        static SlashCommandProperties CreateNewProperties()
        {
            return new SlashCommandBuilder()
                .WithName(name)
                .WithDescription("Debug Command.")
                .Build();
        }

        public async Task OnRequestReceived(Request request)
        {
            await ReviewCommandResponseProvider.HandleResponse(request);
        }
    }
}
