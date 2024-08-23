using Bot.SlashCommands.ResponseHelpers;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.SlashCommands.Shop
{
	public class ShopManager
	{
		public static async Task UpdateShop(ulong? guildId, SocketUser user, string[] parts, EmbedBuilder embedbuilderNew, ComponentBuilder buttonbuilderNew, bool swapItems = true, bool fullDesc = false, bool buyItems = true, bool showInventory = false)
		{
			try
			{
				using var context = DBContextFactory.GetNewContext();

				List<ShopItem> itemReferences = new List<ShopItem>();
				if (showInventory)
				{
					var tempList = context.ItemInventories
					.Where(x => x.userId == user.Id && x.guildId == guildId)
					.ToList();

					foreach (var item in tempList)
					{
						ShopItem shopItem = await context.ShopItems.FirstAsync(x => x.Id == item.itemId);
						itemReferences.Add(shopItem);
					}
				}
				else
				{
					itemReferences = context.ShopItems
					.Where(x => x.GuildId == guildId)
					.ToList();
				}

				var index = int.Parse(parts[1]);
				var CurrencyRef = await context.Currencies.FirstAsync(x => x.Id == itemReferences[index].CurrencyId);
				var EditedDesc = itemReferences[index].Description;

				if (EditedDesc.Length > 20 && !fullDesc)
				{
					EditedDesc = EditedDesc.Substring(0, 20);
					EditedDesc += "...";
				}

				if (showInventory)
				{
					var tempList = context.ItemInventories
					.Where(x => x.userId == user.Id && x.guildId == guildId)
					.ToList();
					embedbuilderNew
						.WithAuthor(user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
						.WithTitle($"{itemReferences[index].ItemName} :{itemReferences[index].emojiId}:")
						.WithDescription($"**ID:** {itemReferences[index].Id} \n " +
										 $"**Currency Type:** {CurrencyRef.Name} \n " +
										 $"**Cost:** {itemReferences[index].Cost} \n " +
										 $"**Description:** {EditedDesc} \n " +
										 $"**Amount:** {tempList[index].amount}")
						.WithColor(Color.Blue)
						.WithCurrentTimestamp();
				}
				else
				{
					embedbuilderNew
						.WithAuthor(user.Username, user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
						.WithTitle($"{itemReferences[index].ItemName} :{itemReferences[index].emojiId}:")
						.WithDescription($"**ID:** {itemReferences[index].Id} \n " +
										 $"**Currency Type:** {CurrencyRef.Name} \n " +
										 $"**Cost:** {itemReferences[index].Cost} \n " +
										 $"**Description:** {EditedDesc}")
						.WithColor(Color.Blue)
						.WithCurrentTimestamp();
				}


				var backCap = false;
				var frontCap = false;

				if (index >= itemReferences.Count - 1)
				{
					frontCap = true;
				}
				else if (index == 0)
				{
					backCap = true;
				}

				if (swapItems && !buyItems)
				{
					buttonbuilderNew
						.WithButton(customId: $"shopBtn_{index - 1}", emote: new Emoji("⬅"), disabled: backCap)
						.WithButton(customId: $"shopBtn_{index + 1}", emote: new Emoji("➡️"), disabled: frontCap);
				}
				else if (buyItems && !swapItems)
				{
					buttonbuilderNew
						.WithButton(customId: $"buyBtn_{itemReferences[index].Id}", emote: new Emoji("🛒"), style: ButtonStyle.Success);
				}
				else if (buyItems && swapItems)
				{
					buttonbuilderNew
						.WithButton(customId: $"shopBtn_{index - 1}", emote: new Emoji("⬅"), disabled: backCap)
						.WithButton(customId: $"shopBtn_{index + 1}", emote: new Emoji("➡️"), disabled: frontCap)
						.WithButton(customId: $"buyBtn_{itemReferences[index].Id}", emote: new Emoji("🛒"), style: ButtonStyle.Success);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex}");
			}
		}
	}
}
