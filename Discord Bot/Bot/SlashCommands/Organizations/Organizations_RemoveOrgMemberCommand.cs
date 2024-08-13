using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Bot.SlashCommands.Organizations
{
    /// <summary>
    /// This command allows a team lead to kick a member out of the organization.
    /// </summary>
    internal class Organizations_RemoveOrgMemberCommand : ISlashCommand
    {
        const string name = "remove_org_member";
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
                });
            });
        }


        async Task<string> GetMessage(SocketSlashCommand command)
        {            
            await DBReadWrite.LockReadWrite();
            try
            {
                using var context = DBContextFactory.GetNewContext();


                // Check if the user that invoked this command is a member of an organization.
                OrganizationMember? member = await context.OrganizationMembers.FirstOrDefaultAsync(x => x.UserId == command.User.Id);
                if (member == null)
                    return "You are not in an organization.";


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


                // Check if the specified user is a member of this organization.
                OrganizationMember? memberInfo = null;
                try
                {
                    memberInfo = context.OrganizationMembers.FirstOrDefault(x => x.UserId == targetUser.Id &&
                                                                                 x.OrganizationId == org.Id);

                    if (memberInfo == null)
                        return $"The specified user is not a member of your organization ({org.Name}).";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: An error occurred:\n\"{ex.Message}\"\n    Inner Exception: \"{(ex.InnerException != null ? ex.InnerException.Message : "")}\"");
                    return $"An error occurred while checking if the target user is a member of the \"{org.Name}\" organization.";
                }


                if (memberInfo.UserId == org.LeaderID)
                    return $"You cannot kick yourself out of the organization. As the team leader, you must promote someone else to be team lead first and then use the leave command.";


                // Kick the target user.
                context.OrganizationMembers.Remove(memberInfo);

                // Save changes to database.
                await context.SaveChangesAsync();


                // Return the succes message.
                return $"<@{targetUser.Id}> has been kicked out of the \"{org.Name}\" organization.";
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
                .WithDescription("Allows a team lead to kick a member out of the organization.")
                .AddOption("member", ApplicationCommandOptionType.User, "The member who will be kicked out", isRequired: true)
                .Build();
        }
    }
}
