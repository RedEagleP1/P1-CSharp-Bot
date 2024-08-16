
using Bot.SlashCommands.ResponseHelpers;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Bot.SlashCommands
{
    internal class SendCommand : ISlashCommand
    {
        const string name = "send";
        readonly SlashCommandProperties properties = CreateNewProperties();

        private DiscordSocketClient client;

        public string Name => name;
        public SlashCommandProperties Properties => properties;

        public SendCommand(DiscordSocketClient client)
        {
            this.client = client;
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
                catch (Exception e)
                {
                    await command.ModifyOriginalResponseAsync(response => response.Content = "Some error occured. Contact developer.");
                    Console.WriteLine(e.Message);
                }
            });
        }

        async Task HandleResponse(SocketSlashCommand command)
        {
            var user = client.GetGuild(Settings.P1RepublicGuildId)?.GetUser(command.User.Id);

            if (!TryExtractOptionValues(command.Data.Options, out var optionValues))
            {
                await command.ModifyOriginalResponseAsync(response => response.Content = "Error. Contact the noob developer of this bot");
                return;
            }

            await DBReadWrite.LockReadWrite();
            try
            {
                using var context = DBContextFactory.GetNewContext();
                var currency = await context.Currencies.FirstOrDefaultAsync(c => c.Name == optionValues.currency);

                //Get Users
                var recipient = await GetCurrencyOwned(context, optionValues.member.Id, currency.Id);
                var sender = await GetCurrencyOwned(context, command.User.Id, currency.Id);

                //Checks
                if (sender.Amount >= optionValues.amount && optionValues.amount >= 0 && optionValues.member.Id != command.User.Id)
                {
                    sender.Amount -= optionValues.amount;
                    recipient.Amount += optionValues.amount;

                    await context.SaveChangesAsync();
                    await command.ModifyOriginalResponseAsync(
                        response => response.Content = $"{optionValues.amount} {optionValues.currency} has been added to user {optionValues.member.Username}");
                }
                else if (sender.Amount < optionValues.amount && optionValues.amount >= 0 && optionValues.member.Id != command.User.Id)
                {
                    await command.ModifyOriginalResponseAsync(
                        response => response.Content = $"You don't have enough {optionValues.currency}");
                }
                else if (optionValues.member.Id != command.User.Id)
                {
					await command.ModifyOriginalResponseAsync(
						response => response.Content = $"You can't give negative currency!");
				}
                else
                {
					await command.ModifyOriginalResponseAsync(
						response => response.Content = $"You can't give currency to yourself!");
				}
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
            if (memberObject == null || memberObject.Value.GetType() != typeof(SocketGuildUser))
            {
                return false;
            }

            optionValues.member = (SocketGuildUser)memberObject.Value;

            var currencyObject = options.FirstOrDefault(option => option.Name == "currency");
            if (currencyObject == null)
            {
                return false;
            }

            optionValues.currency = currencyObject.Value.ToString() ?? string.Empty;

            var amountObject = options.FirstOrDefault(option => option.Name == "amount");
            if (amountObject == null || !float.TryParse(amountObject.Value.ToString(), out optionValues.amount))
            {
                return false;
            }

            return true;
        }
        static async Task<CurrencyOwned> GetCurrencyOwned(ApplicationDbContext context, ulong ownerId, int currencyId)
        {
            var currencyOwned = await context.CurrenciesOwned.FirstOrDefaultAsync(c => c.OwnerId == ownerId && c.CurrencyId == currencyId);
            if (currencyOwned == null)
            {
                currencyOwned = new() { CurrencyId = currencyId, OwnerId = ownerId, Amount = 0 };
                context.CurrenciesOwned.Add(currencyOwned);
            }
            return currencyOwned;
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

            var builder = new SlashCommandBuilder()
                .WithName(name)
                .WithDescription("Send the given member X amount of currency")
                .AddOption("member", ApplicationCommandOptionType.User, "Member who will be sent currency", isRequired: true)
                .AddOption(currencyOptionBuilder)
                .AddOption("amount", ApplicationCommandOptionType.Number, "Amount to be sent.", isRequired: true)
                .Build();

            return builder;
        }

        struct OptionValues
        {
            public SocketGuildUser? member;
            public string currency;
            public float amount;
        }
    }
}
