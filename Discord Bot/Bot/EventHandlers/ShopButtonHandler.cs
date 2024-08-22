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
using System.Threading.Channels;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Bot.EventHandlers
{
    internal class ShopButton : IEventHandler
    {
        private readonly DiscordSocketClient _client;
        public ShopButton(DiscordSocketClient client)
        {
            _client = client;
        }

        public void Subscribe()
        {
            _client.ButtonExecuted += OnButtonExecuted;
        }

        public async Task OnButtonExecuted(SocketMessageComponent component)
        {
            await DBReadWrite.LockReadWrite();
            try
            {
                var parts = component.Data.CustomId.Split('_');

				if (parts[0] == "shopBtn")
                {
					await UpdateShop(component, parts);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: An error occurred while using a shop command.");
            }
            finally
            {
                DBReadWrite.ReleaseLock();
            }
        }
		private async Task UpdateShop(IComponentInteraction component, string[] parts)
		{
			try
			{
				using var context = DBContextFactory.GetNewContext();
				var embedBuilder = new EmbedBuilder();

				List<ShopItem> itemReferences = context.ShopItems
					.Where(x => x.GuildId == component.GuildId)
					.ToList();

				var index = int.Parse(parts[1]);
				var CurrencyRef = await context.Currencies.FirstAsync(x => x.Id == itemReferences[index].CurrencyId);
				var EditedDesc = itemReferences[index].Description;

				if (EditedDesc.Length > 20)
				{
					EditedDesc = EditedDesc.Substring(0, 20);
					EditedDesc += "...";
				}

				embedBuilder
					.WithAuthor(component.User.Username, component.User.GetAvatarUrl() ?? component.User.GetDefaultAvatarUrl())
					.WithTitle($"{itemReferences[index].ItemName} :{itemReferences[index].emojiId}:")
					.WithDescription($"**ID:** {itemReferences[index].Id} \n " +
									 $"**Currency Type:** {CurrencyRef.Name} \n " +
									 $"**Cost:** {itemReferences[index].Cost} \n " +
									 $"**Description:** {itemReferences[index].Description}")
					.WithColor(Color.Blue)
					.WithCurrentTimestamp();

				var backCap = false;
				var frontCap = false;

				if (index >= itemReferences.Count-1)
				{
					frontCap = true;
				}
				else if (index == 0)
				{
					backCap = true;
				}

				var buttonBuilder = new ComponentBuilder()
					.WithButton(customId: $"shopBtn_{index - 1}", emote: new Emoji("⬅"), disabled: backCap)
					.WithButton(customId: $"shopBtn_{index + 1}", emote: new Emoji("➡️"), disabled: frontCap)
					.WithButton(customId: $"buyBtn_{itemReferences[index].Id}", emote: new Emoji("🛒"), style: ButtonStyle.Success);

				await component.RespondAsync(embed: embedBuilder.Build(), components:buttonBuilder.Build());
				await component.Message.DeleteAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex}");
			}
		}
	}
}
