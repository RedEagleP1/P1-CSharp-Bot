using Discord;
using Discord.WebSocket;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Bot.SlashCommands
{
    internal class AwardCommand : ISlashCommand
    {
        const string name = "award";
        readonly SlashCommandProperties properties = CreateNewProperties();

        private DiscordSocketClient client;
        private DBContextFactory dbContextFactory;
        readonly Settings settings;
        static Regex hourRolecheck = new Regex("^T-(\\d+)h$");

        public string Name => name;
        public SlashCommandProperties Properties => properties;

        public AwardCommand(DiscordSocketClient client, DBContextFactory dbContextFactory, Settings settings)
        {
            this.client = client;
            this.dbContextFactory = dbContextFactory;
            this.settings = settings;
        }

        public async Task HandleCommand(SocketSlashCommand command)
        {
            await command.DeferAsync(true);
            _ = Task.Run(async () =>
            {
                try
                {
                    await HandleResponse(command);
                }
                catch(Exception e)
                {
                    await command.ModifyOriginalResponseAsync(response => response.Content = "Some error occured. Contact developer.");
                    Console.WriteLine(e.Message);
                }
            });
        }

        async Task HandleResponse(SocketSlashCommand command)
        {
            var user = client.GetGuild(settings.P1RepublicGuildId)?.GetUser(command.User.Id);
            if (user == null || !HasRoleToAwardHours(user.Roles, out int roleHours))
            {
                await command.ModifyOriginalResponseAsync(response => response.Content = "You are not authorized to use this command.");
                return;
            }

            if (!TryExtractOptionValues(command.Data.Options, out var optionValues))
            {
                await command.ModifyOriginalResponseAsync(response => response.Content = "Error. Contact the noob developer of this bot");
                return;
            }

            await DBReadWrite.LockReadWrite();
            try
            {
                using var context = dbContextFactory.GetNewContext();
                CurrencyAwarder awarder = await GetAwarder(context, command.User.Id, roleHours);

                if (!HasEnoughCurrency(awarder, optionValues.currency, optionValues.amount))
                {
                    await command.ModifyOriginalResponseAsync(response => response.Content = "You don't have enough hours.");
                    return;
                }

                CurrencyOwner owner = await GetOwner(context, optionValues.member.Id);
                AwardCurrency(awarder, owner, optionValues.currency, optionValues.amount);

                await context.SaveChangesAsync();
                await command.ModifyOriginalResponseAsync(
                    response => response.Content = $"{optionValues.amount} {optionValues.currency} has been added to user {optionValues.member.Username}");
            }
            finally
            {
                DBReadWrite.ReleaseLock();
            }            
        }

        static bool TryExtractOptionValues(IReadOnlyCollection<SocketSlashCommandDataOption> options, out OptionValues optionValues)
        {
            optionValues = new OptionValues();
            var memberObject = options.FirstOrDefault(option => option.Name == "member");
            if(memberObject == null || memberObject.Value.GetType() != typeof(SocketGuildUser))
            {
                return false;
            }

            optionValues.member = (SocketGuildUser)memberObject.Value;

            var currencyObject = options.FirstOrDefault(option => option.Name == "currency");
            if(currencyObject == null)
            {
                return false;
            }

            optionValues.currency = currencyObject.Value.ToString() ?? string.Empty;

            var amountObject = options.FirstOrDefault(option => option.Name == "amount");
            if(amountObject == null || !float.TryParse(amountObject.Value.ToString(), out optionValues.amount))
            {
                return false;
            }

            return true;
        }

        static async Task<CurrencyOwner> GetOwner(ApplicationDbContext context, ulong ownerId)
        {
            CurrencyOwner? owner = await context.CurrencyOwners.FindAsync(ownerId);
            if (owner == null)
            {
                owner = new() { Id = ownerId, OCH = 0, SJH = 0 };
                context.CurrencyOwners.Add(owner);
            }
            return owner;
        }
        static async Task<CurrencyAwarder> GetAwarder(ApplicationDbContext context, ulong awarderId, float maxAwardAmount)
        {
            CurrencyAwarder? awarder = await context.CurrencyAwarders.FindAsync(awarderId);
            if (awarder == null)
            {
                awarder = new() { Id = awarderId, OCH = maxAwardAmount, SJH = maxAwardAmount };
                context.CurrencyAwarders.Add(awarder);
            }
            return awarder;
        }

        static bool HasEnoughCurrency(CurrencyAwarder awarder, string currencyName, float amount)
        {
            if(currencyName == "OCH")
            {
                return awarder.OCH > amount;
            }

            if(currencyName == "SJH")
            {
                return awarder.SJH > amount;
            }

            return false;
        }

        static void AwardCurrency(CurrencyAwarder awarder, CurrencyOwner owner, string currencyName, float amount)
        {
            if(currencyName == "OCH")
            {
                owner.OCH += amount;
                awarder.OCH -= amount;
                return;
            }

            if(currencyName == "SJH")
            {
                owner.SJH += amount;
                awarder.SJH -= amount;
                return;
            }
        }

        static bool HasRoleToAwardHours(IReadOnlyCollection<SocketRole> userRoles, out int hours)
        {
            hours = 0;
            bool hasRole = false;
            foreach (SocketRole role in userRoles)
            {
                var match = hourRolecheck.Match(role.Name);
                if (!match.Success)
                    continue;

                hasRole = true;
                hours = int.Parse(match.Groups[1].Value);
                break;
            }

            return hasRole;
        }

        static SlashCommandProperties CreateNewProperties()
        {
            return new SlashCommandBuilder()
                .WithName(name)
                .WithDescription("Award the given member X amount of currency")
                .AddOption("member", ApplicationCommandOptionType.User, "Member who will be awarded", isRequired: true)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("currency")
                    .WithDescription("Name of the currency")
                    .WithRequired(true)
                    .AddChoice("OCH", "OCH")
                    .AddChoice("SJH", "SJH")
                    .WithType(ApplicationCommandOptionType.String))
                .AddOption("amount", ApplicationCommandOptionType.Number, "Amount to be awarded.", isRequired: true)
                .Build();
        }

        struct OptionValues
        {
            public SocketGuildUser? member;
            public string currency;
            public float amount;
        }
    }
}
