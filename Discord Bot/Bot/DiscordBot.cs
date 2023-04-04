using Discord;
using Discord.WebSocket;
using Bot.EventHandlers;
using Models;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Bot.OneTimeRegister;
using Bot.SlashCommands;
using System.Runtime.Serialization.Formatters;
using Bot.PeriodicEvents;
using System.Runtime.CompilerServices;

namespace Bot
{
    public class DiscordBot
    {
        DiscordSocketClient client;
        private readonly Settings settings;
        private TimerCST timerCST;
        public DiscordBot(Settings settings)
        {
            this.settings = settings;
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

        async Task SubscribeToEvents()
        {
            client.Ready -= SubscribeToEvents; //Discord periodically requests a gateway reconnect. The bot may often fire Ready event in such cases. To avoid the function running mutiple times, we unsubscribe here.
            await EnsureCertainCurrenciesExistInDatabase();
            DiscordQueryHelper.Init(client);

            List<ISlashCommand> slashCommands = new()
            {
                new AwardCommand(client, settings),
                new CurrencyCommand(),
                new AccountCommand(client,settings),
                new AccountBackupCommand(client,  settings),
                new ReviewCommand(client,  settings),
                new BuyRoleCommand()
            };

            List<IEventHandler> eventHandlers = new()
            {
                new MemberUpdateHandler(client),
                new CurrentGuildsUpdateHandler(client, slashCommands),
                new GuildRolesChangeHandler(client),
                new SlashCommandHandler(client, slashCommands),
                new RoleSurveyHandler(client),
                new ChannelUpdateHandler(client),
                new VoiceStateUpdateHandler(client),
                new MessageUpdateHandler(client)
            };

            foreach (var handler in eventHandlers)
            {
                if (handler is IReadyHandler readyHandler)
                    readyHandler.OnReady();

                handler.Subscribe();
            }

            timerCST = new TimerCST();
            timerCST.EnablePeriodicEvents();
        }

        async Task EnsureCertainCurrenciesExistInDatabase()
        {
            var context = DBContextFactory.GetNewContext();
            var currencies = context.Currencies.ToList();
            var skyJelliesHours = await context.Currencies.FirstOrDefaultAsync(c => c.Name == "Sky Jellies Hour (SJH)");
            if (skyJelliesHours == null)
            {
                skyJelliesHours = new Currency() { Name = "Sky Jellies Hour (SJH)" };
                context.Currencies.Add(skyJelliesHours);
                await context.SaveChangesAsync();
            }

            var trust = await context.Currencies.FirstOrDefaultAsync(c => c.Name == "Trust");
            if(trust == null)
            {
                trust = new Currency() { Name = "Trust" };
                context.Currencies.Add(trust);
                await context.SaveChangesAsync();
            }
        }
    }
        
}