using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Bot.SlashCommands
{
    internal class CurrencyCommand : ISlashCommand
    {
        const string name = "currency";
        readonly SlashCommandProperties properties = CreateNewProperties();
        private DBContextFactory dbContextFactory;

        public string Name => name;
        public SlashCommandProperties Properties => properties;

        public CurrencyCommand(DBContextFactory dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        public async Task HandleCommand(SocketSlashCommand command)
        {
            await command.DeferAsync(true);
            _ = Task.Run(async () =>
            {
                try
                {
                    string message = await GetMessage(command);
                    await command.User.SendMessageAsync(message);
                    await command.ModifyOriginalResponseAsync(response => response.Content = "I have sent you a direct message. Check it.");
                }
                catch(Exception e)
                {
                    await command.ModifyOriginalResponseAsync(response => response.Content = "Some error occured. Contact developer.");
                    Console.WriteLine(e.Message);
                }
                
            });
        }

        async Task<string> GetMessage(SocketSlashCommand command)
        {
            using var context = dbContextFactory.GetNewContext();
            var allOwners = context.CurrencyOwners.AsNoTracking();
            var owner = await allOwners.FirstOrDefaultAsync(owner => owner.Id == command.User.Id);

            if (owner == null)
            {
                return "You don't own any currency";
            }           

            if(!TryExtractOptions(command.Data.Options, out var optionValues))
            {
                return "Error. Contact the noob developer of this bot";
            }           

            float currencyOwned = GetCurrencyOwned(owner, optionValues.currency);

            if (optionValues.percentage)
            {
                float totalCurrency = GetTotalCurrency(allOwners, optionValues.currency);
                float percentage = totalCurrency > 0 ? (currencyOwned / totalCurrency) * 100 : 0;
                return $"You own {percentage}% of the total awarded {optionValues.currency}";
            }

            return $"You own {currencyOwned} {optionValues.currency}";
        }

        static float GetTotalCurrency(IEnumerable<CurrencyOwner> allOwners, string currencyName)
        {
            float totalCurrency = 0;
            if (currencyName == "OCH")
            {
                foreach (var member in allOwners)
                {
                    totalCurrency += member.OCH;
                }

                return totalCurrency;
            }
            
            if (currencyName == "SJH")
            {
                foreach (var member in allOwners)
                {
                    totalCurrency += member.SJH;
                }

                return totalCurrency;
            }

            return totalCurrency;
        }

        static float GetCurrencyOwned(CurrencyOwner owner, string currencyName)
        {
            if(currencyName == "OCH")
            {
                return owner.OCH;
            }

            if(currencyName == "SJH")
            {
                return owner.SJH;
            }

            return 0;
        }

        static bool TryExtractOptions(IReadOnlyCollection<SocketSlashCommandDataOption> options, out OptionValues optionValues)
        {
            optionValues = new OptionValues();
            var currencyObject = options.FirstOrDefault(option => option.Name == "currency");
            if(currencyObject == null)
            {
                return false;
            }

            optionValues.currency = currencyObject.Value.ToString() ?? string.Empty;

            var percentageObject = options.FirstOrDefault(option => option.Name == "percentage");
            if(percentageObject == null || percentageObject.Value.GetType() != typeof(bool))
            {
                return false;
            }

            optionValues.percentage = (bool)percentageObject.Value;
            return true;
        }
        static SlashCommandProperties CreateNewProperties()
        {
            return new SlashCommandBuilder()
                .WithName(name)
                .WithDescription("Check how much of a particular currency you own")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("currency")
                    .WithDescription("Name of the currency")
                    .WithRequired(true)
                    .AddChoice("OCH", "OCH")
                    .AddChoice("SJH", "SJH")
                    .WithType(ApplicationCommandOptionType.String))
                .AddOption("percentage", ApplicationCommandOptionType.Boolean,
                    "True, to check the percentage owned. False, to check the amount owned.",
                    isRequired: true)
                .Build();
        }

        struct OptionValues
        {
            public string currency;
            public bool percentage;
        }
    }
}
