
using Bot.SlashCommands.ResponseHelpers;
using Bot.SlashCommands.Shop;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models;
using OpenAI_API.Moderation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;
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

        private string response;

        public async Task HandleCommand(SocketSlashCommand command)
        {
            await command.DeferAsync(true);
            _ = Task.Run(async () =>
            {
                response = "Could not find that item in your inventory, make sure you type the name exactly.";

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

				//Now find the automation that has this shop item
                var stringedRef = itemRef.itemId.ToString();
                IdAuto RefAuto = new IdAuto();

                try
                {
					RefAuto = await context.IdAutos.FirstAsync(x => x.Type == 0 && x.SelectedOption == 3 && x.Value == stringedRef);
				}
                catch
                {
                    response = "This item has no effect.";
                }

				var Automation = await context.Automations.FirstAsync(x => x.Id == RefAuto.AutomationId);

				//Check the ifs
				var IfAutos = await context.IdAutos.Where(x => x.AutomationId == Automation.Id && x.Type == 1).ToListAsync();
                bool ifPassed = true;
                string failureMessage = "";
				foreach (var item in IfAutos)
                {
					switch (item.SelectedOption)
					{
						case 0: //Nothing
							break;
						case 1: //Correct role
							var role = user.Guild.Roles.FirstOrDefault(x => x.Id.ToString() == item.Value);
							if (!user.Roles.Contains(role))
                            {
                                ifPassed = false;
								failureMessage += $" You need have {role.Name} role to use this item;";
							}
							break;
						case 2: //Correct channel
							if (command.ChannelId.ToString() != item.Value)
                            {
                                ifPassed = false;
                                failureMessage += $" You need to be in https://discord.com/channels/{command.GuildId}/{item.Value} to use this item;";
                            }
							break;
					}
				}

				await command.ModifyOriginalResponseAsync(async response =>
				{
                    if (ifPassed)
                    {
						response.Content = "Item used.";

                        itemRef.amount -= 1;
                        if (itemRef.amount <= 0)
                        {
                            context.Remove(itemRef);
                        }
                        await context.SaveChangesAsync();
					}
                    else
                    {
                        response.Content = "Item failed to be used used: "+failureMessage;
                    }
				});

				//If successful fire the dos
                if (ifPassed)
                {
					var DoAutos = await context.IdAutos.Where(x => x.AutomationId == Automation.Id && x.Type == 2).ToListAsync();
					foreach (var item in DoAutos)
					{
						switch (item.SelectedOption)
						{
							case 0: //Nothing
								break;
							case 1: //Send a message
								await command.FollowupAsync($"{item.Value}");
								break;
							case 2: //React
								Console.WriteLine(item.Value);
								var reacted = await command.FollowupAsync("Reacted message");
								var customEmote = new Emoji($"{item.Value}");
								await reacted.AddReactionAsync(customEmote);
								break;
							case 3: //Give Role
								var role = user.Guild.Roles.FirstOrDefault(x => x.Id.ToString() == item.Value);
								await user.AddRoleAsync(role);
								break;
							default:
								break;
						}
					}
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
