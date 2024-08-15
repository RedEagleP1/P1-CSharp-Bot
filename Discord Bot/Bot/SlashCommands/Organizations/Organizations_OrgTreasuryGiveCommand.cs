using Bot.SlashCommands.DbUtils;
using Bot.SlashCommands.Organizations;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Bot.SlashCommands.Organizations
{
    /// <summary>
    /// This command allows an organization leader to give currency to a user from the organization's treasury.
    /// </summary>
    internal class Organizations_OrgTreasuryGiveCommand : ISlashCommand
    {
        const string name = "org_treasury_give";
        readonly SlashCommandProperties properties = CreateNewProperties();

        public string Name => name;
        public SlashCommandProperties Properties => properties;


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
            await DBReadWrite.LockReadWrite();
            try
            {
                using var context = DBContextFactory.GetNewContext();


                // Check if the user who invoked this command is in an organization.
                OrganizationMember? member = await UserDataUtils.CheckIfUserIsInAnOrg(command.User.Id, context);
                if (member == null)
                    return "You are not in an organization.";


                // Find the organization.
                Organization? org = context.Organizations.Count() > 0 ? await context.Organizations.FirstOrDefaultAsync(o => o.Id == member.OrganizationId)
                                                                      : null;
                if (org == null)
                    return "Could not find your organization.";


                // Check if this command was invoked by the organization's leader.
                if (command.User.Id != org.LeaderID)
                    return "Only the leader of your organization may use this command.";


                // Try to get the specified user.
                SocketUser? targetUser = null;
                try            
                {
                    var memberObject = command.Data.Options.FirstOrDefault(option => option.Name == "member");
                    if (memberObject == null || memberObject.Value.GetType() != typeof(SocketGuildUser))
                    {
                        return "Failed to get the target user info.";
                    }
                    targetUser = memberObject.Value as SocketGuildUser;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: An error occurred:\n\"{ex.Message}\"\n    Inner Exception: \"{(ex.InnerException != null ? ex.InnerException.Message : "")}\"");
                    return "An error occurred while finding the target user.";                
                }


                // Try to get the amount option.
                SocketSlashCommandDataOption? amountOption = command.Data.Options.FirstOrDefault(x => x.Name == "amount");
                ulong amountToGive = 0;
                if (amountOption != null)
                {
                    // This double cast looks silly, but when I casted directly to ulong it kept crashing with an invalid cast error for some reason.
                    amountToGive = (ulong)(long)amountOption.Value; ;
                }
                else
                {
                    // No amount was provided, so return an error message.
                    return "Please provide the amount you wish to give.";
                }


                // Check if the organization has enough currency in its treasury to give the specified amoung.
                if (org.TreasuryAmount < amountToGive)
                    return "Your organization does not have enough currency to give this amount.";


                // Find the owned currency data for this user.
                CurrencyOwned? currencyOwned = context.CurrenciesOwned.Count() > 0 ? await context.CurrenciesOwned.FirstOrDefaultAsync(x => x.OwnerId == targetUser.Id && x.CurrencyId == OrganizationConstants.CURRENCY_ID)
                                                                                   : null;
         
                // Update the organization's treasury amount.
                org.TreasuryAmount -= amountToGive;
                context.Organizations.Update(org);


                // Give currency to the target user.
                if (currencyOwned != null)
                {
                    currencyOwned.Amount += amountToGive;
                    context.CurrenciesOwned.Update(currencyOwned);
                }
                else
                {
                    currencyOwned = new CurrencyOwned();
                    currencyOwned.OwnerId = targetUser.Id;
                    currencyOwned.CurrencyId = OrganizationConstants.CURRENCY_ID;
                    currencyOwned.Amount = amountToGive;
                    context.CurrenciesOwned.Add(currencyOwned);
                }


                // Save database changes.
                await context.SaveChangesAsync();
            

                // Get the currency name.
                Currency? currency = context.Currencies.Count() > 0 ? await context.Currencies.FirstOrDefaultAsync(c => c.Id == OrganizationConstants.CURRENCY_ID)
                                                                    : null;
                if (currency == null)
                    return "Could not find the currency with this Id.";

                // Return a messaage.
                return $"The organization \"{org.Name}\" gave {amountToGive} {currency.Name} to ({targetUser.Username})!";
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
                .WithDescription("Allows a team lead to give a specified amount of currency to a specified user.")
                .AddOption("member", ApplicationCommandOptionType.User, "The member who will be given currency", isRequired: true)
                .AddOption("amount", ApplicationCommandOptionType.Integer, "The amount to give to the specified user", true)
                .Build();
        }
    }
}
