using Bot.SlashCommands.ResponseHelpers;
using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace Bot.SlashCommands
{
    internal class ReviewCommand : IRespondToButtonsAndModals, INeedAwake
    {
        const string name = "review";
        readonly SlashCommandProperties properties = CreateNewProperties();
        readonly DiscordSocketClient client;
        readonly Settings settings;
        public string Name => name;
        public SlashCommandProperties Properties => properties;
        public ReviewCommand(DiscordSocketClient client, Settings settings)
        {
            this.client = client;
            this.settings = settings;
        }
        public void Awake()
        {
            var guild = client.GetGuild(settings.P1OCGuildId);
            if (guild == null)
            {
                Console.WriteLine("Error: Review command -> P1OC Guild not found.");
                return;
            }
            var postChannel = guild.GetTextChannel(settings.ReviewChannelId);
            if (postChannel == null)
            {
                Console.WriteLine("Error: Can't find the post channel for the review command.");
                return;
            }

            DiscordQueryHelper.SetReviewPostChannel(postChannel);
        }
        public async Task HandleCommand(SocketSlashCommand command)
        {
            if(command.Data.Name != "review")
            {
                return;
            }

            await command.DeferAsync(ephemeral: true);

            _ = Task.Run(async () =>
            {
                try
                {
                    await ReviewCommandResponseProvider.HandleFirstResponse(command);
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
                .WithDescription("Get your tasks reviewed.")
                .Build();
        }

        public async Task OnRequestReceived(Request request)
        {
            await ReviewCommandResponseProvider.HandleResponse(request);
        }
    }
}
