using Bot.SlashCommands.ResponseHelpers;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging.Abstractions;
using Models;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Bot.SlashCommands
{
    internal class AccountCommand : IRespondToButtonsAndModals, INeedAwake
    {
        const string name = "account";
        readonly SlashCommandProperties properties = CreateNewProperties();
        readonly DiscordSocketClient client;
        readonly Settings settings;
        public string Name => name;
        public SlashCommandProperties Properties => properties;
        public AccountCommand(DiscordSocketClient client, Settings settings)
        {
            this.client = client;
            this.settings = settings;
        }
        public void Awake()
        {
            var guild = client.GetGuild(settings.P1OCGuildId);
            if(guild == null)
            {
                Console.WriteLine("Error: Account command -> P1OC Guild not found.");
                return;
            }
            var postChannel = guild.GetTextChannel(settings.AccountsChannelId);
            if(postChannel == null)
            {
                Console.WriteLine("Error: Can't find the post channel for the account command.");
                return;
            }

            DiscordQueryHelper.SetAccountPostChannel(postChannel);
        }
        public async Task HandleCommand(SocketSlashCommand command)
        {
            if(command.Data.Name != "account")
            {
                return;
            }

            await command.DeferAsync(ephemeral: true);

            _ = Task.Run(async () =>
            {
                try
                {
                    await AccountCommandResponseProvider.HandleFirstResponse(command);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
                
            });
        }

        static async Task HandleResponse(Request request)
        {
            await AccountCommandResponseProvider.HandleResponse(request);
        }
        static SlashCommandProperties CreateNewProperties()
        {
            return new SlashCommandBuilder()
                .WithName(name)
                .WithDescription("Account your hours.")
                .Build();
        }

        public async Task OnRequestReceived(Request request)
        {
            await HandleResponse(request);
        }
    }
}
