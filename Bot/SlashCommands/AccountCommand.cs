using Bot.SlashCommands.AccountCommandSpecific;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging.Abstractions;
using Models;
using System.Text.RegularExpressions;

namespace Bot.SlashCommands
{
    internal class AccountCommand : ISlashCommand, INeedAwake
    {
        const string name = "account";
        readonly SlashCommandProperties properties = CreateNewProperties();
        readonly DiscordSocketClient client;
        readonly DBContextFactory dBContextFactory;
        readonly Settings settings;
        public string Name => name;
        public SlashCommandProperties Properties => properties;
        public AccountCommand(DiscordSocketClient client, DBContextFactory dBContextFactory, Settings settings)
        {
            this.client = client;
            this.dBContextFactory = dBContextFactory;
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
            ResponseHelper.postChannel = postChannel;
            ResponseHelper.dbContextFactory = dBContextFactory;
            client.ButtonExecuted += OnButtonExecuted;
            client.ModalSubmitted += OnModalSubmitted;
        }
        async Task OnModalSubmitted(SocketModal modal)
        {
            await modal.DeferAsync(ephemeral: true);
            _ = Task.Run(async () =>
            {
                try
                {
                    await HandleResponse(new Request(modal));
                    //await ResponseHandler.RespondToModalSubmission(modal);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
                
            });            
        }
        Task OnButtonExecuted(SocketMessageComponent component)
        {
            if (component.HasResponded)
            {
                return Task.CompletedTask;
            }

            Task.Run(async () =>
            {
                try 
                {
                    await HandleResponse(new Request(component));
                    //await ResponseHandler.RespondToButtonClick(component); 
                }
                catch(Exception e) 
                { 
                    Console.WriteLine(e);
                }
            });
            
            return Task.CompletedTask;
        }
        public async Task HandleCommand(SocketSlashCommand command)
        {
            await command.DeferAsync(ephemeral: true);

            _ = Task.Run(async () =>
            {
                try
                {
                    await HandleResponse(new Request(command));
                    //await ResponseHandler.SendFirstStep(command);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
                
            });
        }

        static async Task HandleResponse(Request request)
        {
            await request.ProcessRequest();
            await ResponseProvider.GetResponse(request).Invoke(request);
        }
        static SlashCommandProperties CreateNewProperties()
        {
            return new SlashCommandBuilder()
                .WithName(name)
                .WithDescription("Account your hours.")
                .Build();
        }
    }
}
