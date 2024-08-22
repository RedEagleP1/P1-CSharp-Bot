using Bot.SlashCommands.ResponseHelpers;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Migrations;
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
    internal class ShopBuyButton : IEventHandler
    {
        private readonly DiscordSocketClient _client;
        public ShopBuyButton(DiscordSocketClient client)
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

				if (parts[0] == "buyBtn")
				{
					Console.WriteLine("========================================================");
					Console.WriteLine(component.Data.CustomId);
					Console.WriteLine(parts[1]);

					using var context = DBContextFactory.GetNewContext();
					var ShopItem = await context.ShopItems.FirstAsync(x => x.Id == ulong.Parse(parts[1]));

					Console.WriteLine(ShopItem.ItemName);

					var CurrencyName = await context.Currencies.FirstAsync(x => x.Id == ShopItem.CurrencyId);

					Console.WriteLine(CurrencyName.Name);

                    try
                    {
                        var TotalCurrency = await context.CurrenciesOwned.FirstAsync(x => x.OwnerId == component.User.Id && x.CurrencyId == ShopItem.CurrencyId);

                        Console.WriteLine(TotalCurrency.Amount);

                        //Check if they have enough
                        if (ShopItem.Cost <= TotalCurrency.Amount)
                        {
                            await component.RespondAsync($"You have purchased {ShopItem.ItemName} and now have {TotalCurrency.Amount} {CurrencyName.Name}");

							//ItemInventory addItem = await context.ItemInventories.FirstAsync(x => x.userId == component.User.Id && x.guildId == ShopItem.GuildId && x.itemId == ShopItem.Id);

                            //if (addItem != null)
                            {
                                //addItem.amount += 1;
                            }
                            //else

							ItemInventory newItem = new ItemInventory()
							{
								itemId = ShopItem.Id,
								userId = component.User.Id,
								guildId = ShopItem.GuildId,
								amount = 1,
							};

							context.ItemInventories.Add(newItem);

                            try
                            {
								await context.SaveChangesAsync();
							}
                            catch (Exception ex)
                            {
                                Console.WriteLine($"{ex.Message}\n {ex.InnerException}\n");
                            }
						}
                        else
                        {
                            await component.RespondAsync($"You don't have enough {CurrencyName.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
						await component.RespondAsync($"You don't have enough {CurrencyName.Name}");
					}
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
