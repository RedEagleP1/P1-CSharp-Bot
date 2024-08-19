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

				switch (parts[0])
                {
                    case "shopBack":
						await component.DeferAsync();
						await component.ModifyOriginalResponseAsync(async msg =>
						{
							using var context = DBContextFactory.GetNewContext();
							var embedBuilder = new EmbedBuilder();

							List<ShopItem> itemReferences = await context.ShopItems.Where(x => x.GuildId == component.GuildId).ToListAsync();

							var index = int.Parse(parts[1]);
							var CurrencyRef = await context.Currencies.FirstAsync(x => x.Id == itemReferences[index].CurrencyId);
							embedBuilder
							.WithAuthor(component.User.Username, component.User.GetAvatarUrl() ?? component.User.GetDefaultAvatarUrl())
							.WithTitle($"{itemReferences[index].ItemName}")
							.WithDescription($"**ID:** {itemReferences[index].Id} \n " +
											$"**Currency Type:** {CurrencyRef.Name} \n " +
											$"**Cost:** {itemReferences[index].Cost} \n " +
											$"**Description:** {itemReferences[index].Description}")
							.WithColor(Color.Blue)
							.WithCurrentTimestamp();

							var buttonBuilder = new ComponentBuilder()
					            .WithButton(customId: $"shopBack_{index-1}", emote: new Emoji("⬅"))
					            .WithButton(customId: $"shopFore_{index+1}", emote: new Emoji("➡️"));


							msg.Embed = embedBuilder.Build();
							msg.Components = buttonBuilder.Build();
						});
					break;

                    case "shopFore":
						try
						{
							await component.DeferAsync();

							await component.ModifyOriginalResponseAsync(async msg =>
							{
								using var context = DBContextFactory.GetNewContext();
								var embedBuilder = new EmbedBuilder();

								List<ShopItem> itemReferences = context.ShopItems
									.Where(x => x.GuildId == component.GuildId)
									.ToList();

								var index = int.Parse(parts[1]);
								var CurrencyRef = await context.Currencies.FirstAsync(x => x.Id == itemReferences[index].CurrencyId);

								embedBuilder
									.WithAuthor(component.User.Username, component.User.GetAvatarUrl() ?? component.User.GetDefaultAvatarUrl())
									.WithTitle($"{itemReferences[index].ItemName}")
									.WithDescription($"**ID:** {itemReferences[index].Id} \n " +
													$"**Currency Type:** {CurrencyRef.Name} \n " +
													$"**Cost:** {itemReferences[index].Cost} \n " +
													$"**Description:** {itemReferences[index].Description}")
									.WithColor(Color.Blue)
									.WithCurrentTimestamp();

								msg.Embed = embedBuilder.Build();

								msg.Content = " ";
							});
						}
						catch (Exception ex)
						{
							Console.WriteLine($"An error occurred: {ex.Message}");
						}
						break;
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
