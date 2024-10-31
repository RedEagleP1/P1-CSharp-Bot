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

        public string Name => name;
        public SlashCommandProperties Properties => properties;

        public async Task HandleCommand(SocketSlashCommand command)
        {
            await command.DeferAsync(true);
            _ = Task.Run(async () =>
            {
                try
                {
                    string message = await GetMessage(command);
                    try
                    {
                        await command.User.SendMessageAsync(message);
                    }
                    catch (Discord.Net.HttpException exc)
                    {
                        if (exc.DiscordCode != DiscordErrorCode.CannotSendMessageToUser)
                        {
                            Console.WriteLine(exc.ToString());
                        }
                    }
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
            using var context = DBContextFactory.GetNewContext();
            if(!TryExtractOptions(command.Data.Options, out var optionValues))
            {
                return "Error. Contact the noob developer of this bot";
            }

            var currency = await context.Currencies.FirstOrDefaultAsync(c => c.Name == optionValues.currency);
            var currencyOwnedByUser = await context.CurrenciesOwned.FirstOrDefaultAsync(co => co.CurrencyId == currency.Id && co.OwnerId == command.User.Id);
            if(currencyOwnedByUser == null)
            {
                return "You don't own any currency";
            }

            var totalCurrencyInExistence = context.CurrenciesOwned.Where(co => co.CurrencyId == currency.Id).Select(co => co.Amount).Sum();
            if (optionValues.percentage)
            {
                float percentage = totalCurrencyInExistence > 0 ? (currencyOwnedByUser.Amount / totalCurrencyInExistence) * 100 : 0;
                return $"You own {percentage}% of the total awarded {optionValues.currency}";
            }

            return $"You own {currencyOwnedByUser.Amount} {optionValues.currency}";
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
                optionValues.percentage = Settings.CurrencyCommandPercentageOptionDefaultValue;
                return true;
            }

            optionValues.percentage = (bool)percentageObject.Value;
            return true;
        }
        static SlashCommandProperties CreateNewProperties()
        {
            var currencyOptionBuilder = new SlashCommandOptionBuilder()
                .WithName("currency")
                .WithDescription("Name of the currency")
                .WithRequired(true);

            var context = DBContextFactory.GetNewContext();
            var allCurrencies = context.Currencies.Select(c => c.Name).ToList();
            foreach (var c in allCurrencies)
            {
                currencyOptionBuilder.AddChoice(c, c);
            }
            currencyOptionBuilder.WithType(ApplicationCommandOptionType.String);

            return new SlashCommandBuilder()
                .WithName(name)
                .WithDescription("Check how much of a particular currency you own")
                .AddOption(currencyOptionBuilder)
                .AddOption("percentage", ApplicationCommandOptionType.Boolean,
                    "True, to check the percentage owned. False, to check the amount owned.",
                    isRequired: false)
                .Build();
        }

        struct OptionValues
        {
            public string currency;
            public bool percentage;
        }
    }
}
