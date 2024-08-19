using Bot.SlashCommands.DbUtils;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Bot.SlashCommands.Organizations
{
    /// <summary>
    /// This command allows a team lead to have their organization leave its parent legion. Only the team lead may use this command.
    /// </summary>
    internal class Legions_LeaveLegionCommand : ISlashCommand
    {
        const string name = "leave_legion";
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

                // Check if the user that invoked this command is an organization leader.
                Organization? org = await OrgDataUtils.GetOrgFromLeaderId(command.User.Id, context);
                if (org == null)
                    return "You are not an organization leader.";


                // Find the organization's legion membership record.
                LegionMember? legionMember = context.LegionMembers.Count() > 0 ? await context.LegionMembers.FirstOrDefaultAsync(x => x.OrganizationId == org.Id)
                                                            : null;
                if (legionMember == null)
                    return "You cannot leave your legion, because your organization is not a member of one.";

                // Find the legion.
                Legion? legion = context.Legions.Count() > 0 ? await context.Legions.FirstOrDefaultAsync(x => x.Id == legionMember.LegionId)
                                                             : null; 
                if (legion == null)
                    return $"Could not find a legion with Id {legionMember.LegionId}.";


                // Remove this organization from the legion.
                context.LegionMembers.Remove(legionMember);

                // Save changes to database.
                await context.SaveChangesAsync();

                // Return a messaage.
                return $"The \"{org.Name}\" organization is no longer a member of the \"{legion.Name}\" legion.";
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
                .WithDescription("Allows a team lead to have their organization leave its parent legion.")
                .Build();
        }
    }
}
