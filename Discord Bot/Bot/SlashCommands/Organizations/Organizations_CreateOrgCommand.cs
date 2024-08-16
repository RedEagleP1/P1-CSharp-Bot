using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Bot.SlashCommands.Organizations
{
    /// <summary>
    /// This command creates a new organization.
    /// The user who invoked the command will automatically be set as the organization's leader.
    /// </summary>
    internal class Organizations_CreateOrgCommand : ISlashCommand
    {
        const string name = "create_org";
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
                // Check if the user is already in an organization.
                if (!OrganizationConstants.ALLOW_USER_TO_JOIN_MULTIPLE_ORGS)
                {
                    ulong? id = CheckIfUserIsInAnOrg(command).Result;
                    if (id != null)
                        return "You are already in an organization so you cannot create a new one.";
                }

                // Try to get the name option.
                SocketSlashCommandDataOption nameOption = command.Data.Options.FirstOrDefault(x => x.Name == "name");
                if (nameOption == null)
                {
                    return "Please provide a name for the new organization.";
                }

                // Check if the name option contains a valid value.
                string? orgName = nameOption.Value.ToString();
                if (orgName == null || orgName.Length < OrganizationConstants.MIN_ORG_NAME_LENGTH)
                {
                    return $"Please provide a name that is at least {OrganizationConstants.MIN_ORG_NAME_LENGTH} characters long for the new organization.";
                }
                orgName = orgName.Trim();
            
                // Check if there is already an organization with this name.
                using var context = DBContextFactory.GetNewContext();
                Organization? org = await context.Organizations.AsNoTracking().FirstOrDefaultAsync(x => x.Name == orgName);
                if (org != null)
                {
                    return "An organization with this name already exists.";
                }


                // Get the guild Id.
                ulong guildId = 0;
                if (command.GuildId == null)
                    return $"Failed to create new organization \"{orgName}\". GuildId is null.";
                else
                    guildId = (ulong) command.GuildId;

             
                // First, add the new organization to the orgs table.
                Organization newOrg = new Organization();
                newOrg.Name = orgName.Trim();
                newOrg.LeaderID = command.User.Id;
                newOrg.CurrencyId = OrganizationConstants.CURRENCY_ID;
                newOrg.GuildId = guildId;
                newOrg.MaxMembers = OrganizationConstants.MAX_ORG_MEMBERS;
                context.Organizations.Add(newOrg);

                await context.SaveChangesAsync();

                // Get the id of the new org.
                Organization result = await context.Organizations.FirstOrDefaultAsync(x => x.Name == orgName);
                if (result == null)
                    return $"Failed to create new organization \"{orgName}\".";

                // Now, add an entry to the org members table.
                OrganizationMember member = new OrganizationMember();
                member.OrganizationId = result.Id;
                member.UserId = command.User.Id;
                context.OrganizationMembers.Add(member);

                // Save changes to database.
                await context.SaveChangesAsync();

                return $"Created new organization \"{orgName}\" led by you.";

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
        
        /// <summary>
        /// Checks if the user is in an organization.
        /// </summary>
        /// <param name="command">The command object.</param>
        /// <returns>null if the user is not in an organization, or the id of the organization they are in.</returns>
        async Task<ulong?> CheckIfUserIsInAnOrg(SocketSlashCommand command)
        {
            using var context = DBContextFactory.GetNewContext();
            OrganizationMember? member = await context.OrganizationMembers.FirstOrDefaultAsync(x => x.UserId == command.User.Id);
            if (member == null)
            {
                return null;
            }
            else
            {
                return member.OrganizationId;
            }
        }

        static SlashCommandProperties CreateNewProperties()
        {
            return new SlashCommandBuilder()
                .WithName(name)
                .WithDescription("Create a new organization with you as its leader")
                .AddOption("name", ApplicationCommandOptionType.String, "The name of the new organization", true)
                .Build();
        }
    }
}
