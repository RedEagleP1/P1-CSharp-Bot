using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Bot.SlashCommands.Organizations
{
    /// <summary>
    /// This command allows a user to donate to the organization they are a member of.
    /// </summary>
    internal class Organizations_DonateToOrgCommand : ISlashCommand
    {
        const string name = "donate_to_org";
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

                OrganizationMember? member = await context.OrganizationMembers.FirstOrDefaultAsync(x => x.UserId == command.User.Id);
                if (member == null)
                    return "You cannot donate to your organization since you are not in one.";


                // Try to get the amount option.
                SocketSlashCommandDataOption? amountOption = command.Data.Options.FirstOrDefault(x => x.Name == "amount");
                ulong amountToDonate = 0;
                if (amountOption != null)
                {
                    // This double cast looks silly, but when I casted directly to ulong it kept crashing with an invalid cast error for some reason.
                    amountToDonate = (ulong)(long)amountOption.Value; ;
                }
                else
                {
                    // No amount was provided, so return an error message.
                    return "Please provide the amount you wish to donate.";
                }


                // Check if the user has enough currency to donate the specified amount.
                CurrencyOwned? currencyOwned = await context.CurrenciesOwned.FirstOrDefaultAsync(x => x.OwnerId == command.User.Id && 
                                                                                                      x.CurrencyId == OrganizationConstants.CURRENCY_ID);
                float amountOwned = 0;
                if (currencyOwned != null)
                    amountOwned = currencyOwned.Amount;
                if (amountOwned < amountToDonate)
                    return "You do not have enough currency to make this donation.";


                // Update the user's owned currency.
                if (currencyOwned != null)
                {
                    currencyOwned.Amount -= amountToDonate;
                    context.CurrenciesOwned.Update(currencyOwned);
                }
                else
                {
                    currencyOwned = new CurrencyOwned
                    {
                        OwnerId = command.User.Id,
                        CurrencyId = OrganizationConstants.CURRENCY_ID,
                        Amount = currencyOwned.Amount
                    };
                    context.CurrenciesOwned.Add(currencyOwned);
                }


                // Find the organization.
                Organization? org = null;
                try
                {
                    org = await context.Organizations.FirstOrDefaultAsync(o => o.Id == member.OrganizationId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: An error occurred:\n\"{ex.Message}\"\n    Inner Exception: \"{(ex.InnerException != null ? ex.InnerException.Message : "")}\"");
                    return "An error occurred while finding the organization.";
                }
                if (org == null)
                    return "Could not find your organization.";


                org.TreasuryAmount += amountToDonate;

                context.Organizations.Update(org);

                // Save database changes.
                await context.SaveChangesAsync();
            

                // Get the currency name.
                Currency? currency = await context.Currencies.FirstOrDefaultAsync(c => c.Id == OrganizationConstants.CURRENCY_ID);
                if (currency == null)
                    return "Could not find the currency with this Id.";

                // Return a messaage.
                return $"You donated {amountToDonate} {currency.Name} to your organization ({org.Name})!";
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
                .WithDescription("Donate a specified amount of currency to your organization.")
                .AddOption("amount", ApplicationCommandOptionType.Integer, "The amount to donate to your organization", true)
                .Build();
        }
    }
}
