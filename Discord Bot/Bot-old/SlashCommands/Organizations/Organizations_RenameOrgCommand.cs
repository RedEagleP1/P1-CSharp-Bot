using Bot.SlashCommands.DbUtils;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Security.Cryptography;

namespace Bot.SlashCommands.Organizations
{
    /// <summary>
    /// This command renames the organization of the team lead who invoked it.
    /// Only the team lead may use this command.
    /// </summary>
    internal class Organizations_RenameOrgCommand : ISlashCommand
    {
        const string name = "rename_org";
        readonly SlashCommandProperties properties = CreateNewProperties();

        public string Name => name;
        public SlashCommandProperties Properties => properties;

        private bool hadError = false;


        public async Task HandleCommand(SocketSlashCommand command)
        {           
            await command.DeferAsync();
            _ = Task.Run(async () =>
            {
                string message = await GetMessage(command);

                await command.ModifyOriginalResponseAsync(response =>
                {
                    response.Content = message;
                    response.Flags = hadError ? MessageFlags.Ephemeral : MessageFlags.None;
                });

            });
        }

        async Task<string> GetMessage(SocketSlashCommand command)
        {
            hadError = true;


            // Try to get the name option.
            SocketSlashCommandDataOption? nameOption = command.Data.Options.FirstOrDefault(x => x.Name == "name");
            if (nameOption == null)
                return "Please provide a new name for the organization.";

            // Check if the name option contains a valid value.
            string? newOrgName = nameOption.Value.ToString();
            if (newOrgName == null || newOrgName.Length < Organization.DEFAULT_MIN_ORG_NAME_LENGTH)
            {
                return $"Please provide a new name that is at least {Organization.DEFAULT_MIN_ORG_NAME_LENGTH} characters long for the organization.";
            }
            newOrgName = newOrgName.Trim();


            await DBReadWrite.LockReadWrite();
            try
            {
                using var context = DBContextFactory.GetNewContext();

                // Check if the user who invoked this command is in an organization.
                OrganizationMember? member = await UserDataUtils.CheckIfUserIsInAnOrg(command.User.Id, context);
                if (member == null)
                    return "You are not in an organization.";


                // Check if there is already an organization with this name.
                Organization? existingOrg = context.Organizations.Count() > 0 ? await context.Organizations.AsNoTracking().FirstOrDefaultAsync(x => x.Name == newOrgName)
                                                                              : null;
                if (existingOrg != null)
                    return "An organization with this name already exists.";


                // Find the organization that's being renamed.
                Organization? org = context.Organizations.Count() > 0 ? await context.Organizations.FirstOrDefaultAsync(x => x.Id == member.OrganizationId)
                                                                      : null;
                if (org == null)
                    return "Failed to rename your organization, as it could not be found.";


                if (command.User.Id != org.LeaderID)
                    return "Only the leader of your organization may use this command.";


                // Change the organization's name.
                string oldName = org.Name;
                org.Name = newOrgName;
                context.Organizations.Update(org);

                // Save changes to database.
                await context.SaveChangesAsync();
               
                hadError = false;
                return $"The organization \"{oldName}\" has been renamed to \"{newOrgName}\".";

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
                .WithDescription("Allows the team lead to rename the organization.")
                .AddOption("name", ApplicationCommandOptionType.String, "The new name of the organization", true)
                .Build();
        }
    }
}
