using Discord;
using Discord.WebSocket;
using Bot.EventHandlers;
using Models;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Bot.OneTimeRegister;
using Bot.SlashCommands;

namespace Bot
{
    public class DiscordBot
    {
        DiscordSocketClient client;
        private readonly Settings settings;
        private readonly DBContextFactory dbContextFactory;
        public DiscordBot(Settings settings, DBContextFactory dbContextFactory)
        {
            this.settings = settings;
            this.dbContextFactory = dbContextFactory;
        }

        public async Task StartBot()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All
            });
            client.Log += LogMessage;

            await client.LoginAsync(TokenType.Bot, settings.DiscordBotToken);
            await client.StartAsync();
            client.Ready += SubscribeToEvents;
        }

        Task LogMessage(LogMessage logMessage)
        {
            Console.WriteLine(logMessage);
            return Task.CompletedTask;
        }

        Task SubscribeToEvents()
        {
            List<ISlashCommand> slashCommands = new()
            {
                new AwardCommand(client, dbContextFactory, settings),
                new CurrencyCommand(dbContextFactory),
                new AccountCommand(client, dbContextFactory,settings),
                new AccountBackupCommand(client, dbContextFactory, settings)
            };

            List<IEventHandler> eventHandlers = new()
            {
                new MemberUpdateHandler(client, dbContextFactory),
                new CurrentGuildsUpdateHandler(client, dbContextFactory, slashCommands),
                new GuildRolesChangeHandler(client, dbContextFactory),
                new SlashCommandHandler(client, slashCommands)
            };

            foreach (var handler in eventHandlers)
            {
                if (handler is IReadyHandler readyHandler)
                    readyHandler.OnReady();

                handler.Subscribe();
            }

            return Task.CompletedTask;
        }
    }
        
}