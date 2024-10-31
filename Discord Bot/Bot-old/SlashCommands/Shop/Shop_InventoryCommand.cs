using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Migrations;
using System.ComponentModel;
using System.IO;

namespace Bot.SlashCommands.Shop
{
    /// <summary>
    /// This command displays shop items.
    /// </summary>
    internal class Shop_InventoryCommand : ISlashCommand
    {
        const string name = "inventory";
        readonly SlashCommandProperties properties = CreateNewProperties();

        private DiscordSocketClient client;

        public string Name => name;
        public SlashCommandProperties Properties => properties;


        public Shop_InventoryCommand(DiscordSocketClient client)
        {
            this.client = client;
        }

        public async Task HandleCommand(SocketSlashCommand command)
        {
            await command.DeferAsync(true);
            _ = Task.Run(async () =>
            {
                string message = await GetMessage(command);

                await command.ModifyOriginalResponseAsync(response =>
                {
                    response.Content = message;
                    response.Flags = MessageFlags.Ephemeral;
                });

            });
        }


        async Task<string> GetMessage(SocketSlashCommand command)
        {
            // First, check if the user has permission to use this command.
            var user = client.GetGuild(Settings.P1RepublicGuildId)?.GetUser(command.User.Id);
            if (user == null)
                return "Could not find user info.";


            await DBReadWrite.LockReadWrite();
            try
            {

                using var context = DBContextFactory.GetNewContext();

				try
                {
                    string[] temp = {"","0"};
					var buttonBuilder = new ComponentBuilder();
					var embedBuilder = new EmbedBuilder();

					await ShopManager.UpdateShop(command.GuildId, command.User, temp, embedBuilder, buttonBuilder,true,true,false,true);

					await command.ModifyOriginalResponseAsync(response =>
					{
						response.Content = "";
						response.Embed = embedBuilder.Build();
						response.Components = buttonBuilder.Build();
						response.Flags = MessageFlags.Ephemeral;
					});
				}
                catch (Exception ex)
                {
                    return "You have no items, type /shop to buy some.";
                }

                // This causes the message content to be set to null. We don't need it since we are using an embed for the content.
                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: An error occurred while trying to write to the database:\n\"{ex.Message}\"\n    Inner Exception: \"{(ex.InnerException != null ? ex.InnerException.Message : "")}\"");
                return $"An error occurred while trying to access the database.";
            }
            finally
            {
                DBReadWrite.ReleaseLock();
            }
        }

		static SlashCommandProperties CreateNewProperties()
		{
			return new SlashCommandBuilder()
				.WithName(name)
				.WithDescription("Look at your inventory.")
				.Build();
		}
	}
}
