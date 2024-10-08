﻿using System.Threading;

using Discord;
using Discord.WebSocket;

using Microsoft.EntityFrameworkCore;

using Models;

using Bot.EventHandlers;
using Bot.OneTimeRegister;
using Bot.PeriodicEvents;
using Bot.SlashCommands;
using Bot.SlashCommands.Organizations;
using Bot.SlashCommands.Shop;
using Bot.PeriodicEvents;
using Bot.SlashCommands.Legions;

namespace Bot
{
    public class DiscordBot
    {
        public DiscordSocketClient client { get; private set; }
        private TimerCST timerCST;

        public async Task StartBot()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All,
                LogLevel = (LogSeverity)Settings.LogLevel
            });
            client.Log += LogMessage;

            await client.LoginAsync(TokenType.Bot, Settings.DiscordBotToken);
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
                new AccountBackupCommand(client),
                new AccountCommand(client),
                new AwardCommand(client),
                new BuyRoleCommand(),
                new CurrencyCommand(),
                //new DebugCommand(client),
                new ReviewCommand(client),
				new SendCommand(client),

                // ----------------------------------------------------------------------------------------------------
                // Legion Commands
                // ----------------------------------------------------------------------------------------------------
                new Legions_CreateLegionCommand(),
                new Legions_DeleteLegionCommand(client),
                new Legions_JoinLegionCommand(client),
                new Legions_KickLegionOrgCommand(),
                new Legions_LeaveLegionCommand(),
                new Legions_LegionInfoCommand(client),
                new Legions_PingLegionCommand(client),
                new Legions_PromoteLegionMemberCommand(),
                new Legions_RenameLegionCommand(),

                // ----------------------------------------------------------------------------------------------------
                // Organizations Commands
                // ----------------------------------------------------------------------------------------------------
                new Organizations_CreateOrgCommand(),
                new Organizations_DeleteOrgCommand(client),
                new Organizations_DonateToOrgCommand(),
                new Organizations_JoinOrgCommand(client),
                new Organizations_KickOrgMemberCommand(),
                new Organizations_LeaveOrgCommand(),
                new Organizations_OrgInfoCommand(client),
                new Organizations_OrgTreasuryGiveCommand(),
				new Organizations_PingOrgCommand(client),
				new Organizations_PromoteOrgMemberCommand(),
				new Organizations_RenameOrgCommand(),

                // ----------------------------------------------------------------------------------------------------
                // Shop Commands
                // ----------------------------------------------------------------------------------------------------
                new Shop_DisplayCommand(client),
                new Shop_DisplayItemCommand(client),
				new Shop_InventoryCommand(client),
                new Shop_UseItemCommand(client),
                };

            List<IEventHandler> eventHandlers = new()
            {
                new ChannelUpdateHandler(client),
                new CurrentGuildsUpdateHandler(client, slashCommands),
                new GuildRolesChangeHandler(client),
                new LegionJoinRequestHandler(client),
                new MemberUpdateHandler(client),
                new MessageUpdateHandler(client),
				new ShopButton(client),
                new ShopBuyButton(client),
				new OrganizationJoinRequestHandler(client),
				new RoleSurveyHandler(client),
                new SlashCommandHandler(client, slashCommands),
                new VoiceStateUpdateHandler(client),
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
            var currencies = await context.Currencies.ToListAsync();
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