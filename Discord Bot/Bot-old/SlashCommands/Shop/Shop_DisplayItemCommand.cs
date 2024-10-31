
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
    internal class Shop_DisplayItemCommand : ISlashCommand
    {
        const string name = "item";
        readonly SlashCommandProperties properties = CreateNewProperties();

        private DiscordSocketClient client;

        public string Name => name;
        public SlashCommandProperties Properties => properties;

        public Shop_DisplayItemCommand(DiscordSocketClient client)
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
                    await command.ModifyOriginalResponseAsync(response => response.Content = "Could not find that item, make sure you type the name exactly.");
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

                var itemReferences = context.ShopItems
					.Where(x => x.GuildId == command.GuildId)
					.ToList();

                int index = 0;
                foreach ( var item in itemReferences )
                {
                    if (optionValues.itemName == item.ItemName)
                    {
                        index = itemReferences.IndexOf(item);
                    }
                }

				string[] temp = { "", index.ToString() };
				var buttonBuilder = new ComponentBuilder();
				var embedBuilder = new EmbedBuilder();

				await ShopManager.UpdateShop(command.GuildId, command.User, temp, embedBuilder, buttonBuilder,false,true);

				//Create response
				await command.ModifyOriginalResponseAsync(response =>
                {
                    response.Content = "";
                    response.Embed = embedBuilder.Build();
                    response.Flags = MessageFlags.Ephemeral;
					response.Components = buttonBuilder.Build();
				});
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
