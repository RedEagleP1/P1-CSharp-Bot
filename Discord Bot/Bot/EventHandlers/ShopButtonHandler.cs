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
using Bot.SlashCommands.Shop;

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
                    var buttonBuilder = new ComponentBuilder();
					var embedBuilder = new EmbedBuilder();

					await ShopManager.UpdateShop(component.GuildId,component.User,parts, embedBuilder, buttonBuilder);
					await component.RespondAsync(embed: embedBuilder.Build(), components: buttonBuilder.Build());
					await component.Message.DeleteAsync();
				}
				else if (parts[0] == "invBtn")
				{
					var buttonBuilder = new ComponentBuilder();
					var embedBuilder = new EmbedBuilder();

					await ShopManager.UpdateShop(component.GuildId, component.User, parts, embedBuilder, buttonBuilder, true, true, false, true);
					await component.RespondAsync(embed: embedBuilder.Build(), components: buttonBuilder.Build());
					await component.Message.DeleteAsync();
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
	}
}
