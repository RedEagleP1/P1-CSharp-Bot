using Bot.SlashCommands.ResponseHelpers;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.SlashCommands
{
    internal class BuyRoleCommand : IRespondToButtonsAndModals
	{
		static readonly string name = "buyrole";
		static readonly SlashCommandProperties properties = CreateNewProperties();	

		public string Name => name;

		public SlashCommandProperties Properties => properties;

		public async Task HandleCommand(SocketSlashCommand command)
		{
            if (command.Data.Name != "buyrole")
            {
                return;
            }

            await command.DeferAsync(ephemeral: true);

            _ = Task.Run(async () =>
            {
                try
                {
                    var content = "Which role do you want to buy?";
                    var context = DBContextFactory.GetNewContext();
                    var builder = new EmbedBuilder().WithTitle("Roles For Sale");
                    var embedDescription = "";
                    List<string> rolesForSaleNames = new();
                    foreach(var rfs in context.RolesForSale.AsNoTracking().ToList())
                    {
                        var role = await context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == rfs.RoleId);
                        if(role.GuildId == command.GuildId)
                        {
                            var costAndReward = await context.RolesCostAndReward.FirstOrDefaultAsync(r => r.RoleId == role.Id);
                            string costAndRewardString = "";
                            if(costAndReward != null)
                            {
                                if(costAndReward.CostCurrencyId != null)
                                {
                                    var costCurrency = await context.Currencies.FirstOrDefaultAsync(c => c.Id == costAndReward.CostCurrencyId);
                                    costAndRewardString = $"Cost: {costAndReward.Cost} {costCurrency.Name}";
                                }
                                if(costAndReward.RewardCurrencyId != null)
                                {
                                    var rewardCurrency = await context.Currencies.FirstOrDefaultAsync(c => c.Id == costAndReward.RewardCurrencyId);
                                    costAndRewardString += $" Reward: {costAndReward.Reward} {rewardCurrency.Name}";
                                }
                            }

                            embedDescription += $"<@&{role.Id}> " + costAndRewardString + "\n";
                            rolesForSaleNames.Add(role.Name);
                        }
                    }

                    builder.WithDescription(embedDescription);

                    var component = MessageComponentAndEmbedHelper.CreateButtons(rolesForSaleNames.ToArray());
                    await command.ModifyOriginalResponseAsync(response =>
                    {
                        response.Content = content;
                        response.Embed = builder.Build();
                        response.Components = component;
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            });
        }

		public async Task OnRequestReceived(Request request)
		{
            var user = await DiscordQueryHelper.GetSocketGuildUserAsync(request.GuildId.Value, request.User.Id);
            bool canBuy = true; ;
            var context = DBContextFactory.GetNewContext();
            var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == request.IncomingValue);
            if(user.Roles.Any(r => r.Id == role.Id))
            {
                await request.UpdateOriginalMessageAsync("You already have that role", null, null);
                return;
            }
            await DBReadWrite.LockReadWrite();
            try
            {
                var costAndReward = await context.RolesCostAndReward.FirstOrDefaultAsync(r => r.RoleId == role.Id);
                CurrencyOwned costCurrencyOwned = null;
                if (costAndReward != null)
                {
                    if (costAndReward.CostCurrencyId != null)
                    {
                        costCurrencyOwned = await context.CurrenciesOwned.FirstOrDefaultAsync(co => co.CurrencyId == costAndReward.CostCurrencyId && co.OwnerId == user.Id);
                        canBuy = !((costCurrencyOwned == null && costAndReward.Cost > 0) || (costCurrencyOwned != null && costAndReward.Cost > costCurrencyOwned.Amount));
                    }
                }

                if (canBuy)
                {
                    if(costAndReward != null)
                    {
                        if (costCurrencyOwned != null)
                        {
                            costCurrencyOwned.Amount -= costAndReward.Cost;
                        }

                        if (costAndReward.RewardCurrencyId != null)
                        {
                            var rewardCurrencyOwned = await context.CurrenciesOwned.FirstOrDefaultAsync(co => co.CurrencyId == costAndReward.RewardCurrencyId && co.OwnerId == user.Id);
                            if (rewardCurrencyOwned == null)
                            {
                                rewardCurrencyOwned = new() { CurrencyId = costAndReward.RewardCurrencyId.Value, OwnerId = user.Id, Amount = 0 };
                                context.CurrenciesOwned.Add(rewardCurrencyOwned);
                            }

                            rewardCurrencyOwned.Amount += costAndReward.Reward;
                        }

                        await context.SaveChangesAsync();
                    }
                    
                    await user.AddRoleAsync(role.Id);
                    await request.UpdateOriginalMessageAsync($"You have succefully bought the role <@&{role.Id}>", null, null);
                    return;
                }

                await request.UpdateOriginalMessageAsync("You don't have enough currency to buy that role.", null, null);
            }
            finally
            {
                DBReadWrite.ReleaseLock();
            }
            
        }
        private static SlashCommandProperties CreateNewProperties()
        {
            return new SlashCommandBuilder()
                .WithName(name)
                .WithDescription("Buy a role.")
                .Build();
        }
    }
}
