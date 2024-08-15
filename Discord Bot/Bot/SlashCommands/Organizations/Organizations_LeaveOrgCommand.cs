using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Bot.SlashCommands.Organizations
{
    /// <summary>
    /// This command allows a user to leave their organization. It will fail if the team lead uses it. They
    /// must first promote someone else to be team lead before they can leave the organization.
    /// </summary>
    internal class Organizations_LeaveOrgCommand : ISlashCommand
    {
        const string name = "leave_org";
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

                // Check if the user that invoked this command is in an organization.
                OrganizationMember? member = await context.OrganizationMembers.FirstOrDefaultAsync(x => x.UserId == command.User.Id);
                if (member == null)
                    return "You cannot leave since you are not in an organization.";


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


                // Check if this command was invoked by the organization's leader.
                if (command.User.Id == org.LeaderID)
                    return "As the team lead, you must first promote someone else to be team lead before you can use this command.";


                // Find the user's membership record for this organization.
                OrganizationMember? orgMember = await context.OrganizationMembers.FirstOrDefaultAsync(x => x.UserId == command.User.Id &&
                                                                                                           x.OrganizationId == org.Id);
                if (orgMember == null)
                    return "You cannot leave your organization, because you are not a member of one.";


                // Remove this user from the organization.
                context.OrganizationMembers.Remove(orgMember);

                // Save changes to database.
                await context.SaveChangesAsync();

                // Return a messaage.
                return $"You are no longer a member of the \"{org.Name}\" organization.";
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
                .WithDescription("Allows you to leave your current organization.")
                .Build();
        }
    }
}
