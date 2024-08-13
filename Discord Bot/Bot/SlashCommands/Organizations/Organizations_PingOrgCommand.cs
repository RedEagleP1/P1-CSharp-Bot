using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Text;

namespace Bot.SlashCommands.Organizations
{
    /// <summary>
    /// Pings all members in the organization of the user who invoked this command.
    /// </summary>
    internal class Organizations_PingOrgCommand : ISlashCommand
    {
        const string name = "ping_org";
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
                OrganizationMember? member = await context.OrganizationMembers.FirstOrDefaultAsync(x => x.UserId == command.User.Id);
                if (member == null)
                    return "You cannot ping everyone in your organization since you are not a member of any organization.";


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


                var members = await context.OrganizationMembers.Where(m => m.OrganizationId == org.Id).ToListAsync();
                if (members == null)
                    return "Failed to get a list of all members of the organization";
                else if (members.Count < 1)
                    return "There are no members in this organization. There is an error in the database since this should not be possible.";

                // Build and return the succes message.
                hadError = false;
                StringBuilder b = new StringBuilder();
                b.AppendLine($"<@{command.User.Id}> pinged the \"{org.Name}\" organization:");

                var last = members.Last();
                foreach (var orgMember in members)
                {
                    if (orgMember.UserId == command.User.Id)
                        continue;

                    bool isLast = orgMember.Equals(last);
                    b.AppendLine($"<@{orgMember.UserId}>" + (isLast ? "" : "\n"));
                }

                return b.ToString();
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
                .WithDescription("Allows a user to ping everyone in their organization.")
                .Build();
        }
    }
}
