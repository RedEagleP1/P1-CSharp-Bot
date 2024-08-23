using Bot.SlashCommands.DbUtils;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Bot.SlashCommands.Organizations
{
    /// <summary>
    /// This command allows a team lead to promote someone else to be the team leader.
    /// </summary>
    internal class Organizations_PromoteOrgMemberCommand : ISlashCommand
    {
        const string name = "promote_org_member";
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

            await DBReadWrite.LockReadWrite();
            try
            {
                using var context = DBContextFactory.GetNewContext();


                // Check if the user that invoked this command is a member of an organization.
                OrganizationMember? member = await UserDataUtils.CheckIfUserIsInAnOrg(command.User.Id, context);
                if (member == null)
                    return "You are not in an organization.";


                // Find the organization.
                Organization? org = context.Organizations.Count() > 0 ? await context.Organizations.FirstOrDefaultAsync(o => o.Id == member.OrganizationId)
                                                                      : null;
                if (org == null)
                    return $"Could not find an organization with Id {member.OrganizationId}.";


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
                OrganizationMember? memberInfo = context.OrganizationMembers.Count() > 0 ? context.OrganizationMembers.FirstOrDefault(x => x.UserId == targetUser.Id && x.OrganizationId == org.Id)
                                                                                         : null;

                if (memberInfo == null)
                    return $"The specified user is not a member of the \"{org.Name}\" organization.";


                if (memberInfo.UserId == org.LeaderID)
                    return $"You can't promote yourself. You already are the team lead of the \"{org.Name}\" organization.";


                // Promote the target user.
                org.LeaderID = memberInfo.UserId;
                context.Organizations.Update(org);

                // Save changes to database.
                await context.SaveChangesAsync();


                // Return the succes message.
                hadError = false;
                return $"<@{targetUser.Id}> has been promoted to be the team lead of the \"{org.Name}\" organization!";
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
                .WithDescription("Allows a team lead to promote someone else to be the new team lead.")
                .AddOption("member", ApplicationCommandOptionType.User, "The member who will be promoted", isRequired: true)
                .Build();
        }
    }
}
