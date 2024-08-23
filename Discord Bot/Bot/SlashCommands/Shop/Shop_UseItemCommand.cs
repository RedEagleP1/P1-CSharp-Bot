
using Bot.SlashCommands.ResponseHelpers;
using Bot.SlashCommands.Shop;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Bot.SlashCommands
{
    internal class Shop_UseItemCommand : ISlashCommand
    {
        const string name = "use";
        readonly SlashCommandProperties properties = CreateNewProperties();

        private DiscordSocketClient client;

        public string Name => name;
        public SlashCommandProperties Properties => properties;

        public Shop_UseItemCommand(DiscordSocketClient client)
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
                    await command.ModifyOriginalResponseAsync(response => response.Content = "Could not find that item in your inventory, make sure you type the name exactly.");
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

				//Find the item in the inventory
                var shopItem = await context.ShopItems.FirstAsync(x => x.ItemName == optionValues.itemName);
                var itemRef = await context.ItemInventories.FirstAsync(x => x.userId == command.User.Id && x.guildId == command.GuildId && x.itemId == shopItem.Id);

                //Now check for automations for the item
                //var itemAutos = await context.
			}
            finally
            {
                DBReadWrite.ReleaseLock();
            }
        }

		static bool TryExtractOptionValues(IReadOnlyCollection<SocketSlashCommandDataOption> options, out OptionValues optionValues)
		{
			optionValues = new OptionValues();

			var shopItem = options.FirstOrDefault(option => option.Name == "name");
			if (shopItem == null)
			{
				return false;
			}

			optionValues.itemName = shopItem.Value.ToString() ?? string.Empty;

			return true;
		}

		static SlashCommandProperties CreateNewProperties()
        {
            var builder = new SlashCommandBuilder()
                .WithName(name)
                .WithDescription("Get data on a specific item by putting in the name of it.")
				.AddOption("name", ApplicationCommandOptionType.String, "Name of the item.", isRequired: true)
				.Build();

            return builder;
        }

        struct OptionValues
        {
            public string itemName;
        }
    }
}
