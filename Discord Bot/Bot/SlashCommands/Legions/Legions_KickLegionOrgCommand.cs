using Bot.SlashCommands.DbUtils;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Bot.SlashCommands.Organizations
{
    /// <summary>
    /// This command allows a legion leader to kick an organization out of the legion.
    /// </summary>
    internal class Legions_KickLegionOrgCommand : ISlashCommand
    {
        const string name = "kick_legion_org";
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


                // Check if the user that invoked this command is a legion leader.
                Legion? legion = await UserDataUtils.CheckIfUserIsALegionLeader(command.User.Id, context);
                if (legion == null)
                    return "You are not a legion leader.";


                // Try to get the id option.
                SocketSlashCommandDataOption? idOption = command.Data.Options.FirstOrDefault(x => x.Name == "id");
                ulong orgId = 0;
                if (idOption == null)
                {
                    return "Please provide the Id of organization you want to kick.";
                }
                else
                {
                    // This double cast looks silly, but when I casted directly to ulong it kept crashing with an invalid cast error for some reason.
                    orgId = (ulong)(long)idOption.Value;
                }

                // Find the organization's membership record.
                LegionMember? legionMember = context.LegionMembers.Count() > 0 ? await context.LegionMembers.FirstOrDefaultAsync(lm => lm.OrganizationId == orgId)
                                                                               : null;
                if (legionMember == null)
                    return "Your legion does not contain any member organization with that Id.";

                // Find the organization.
                Organization? organization = await context.Organizations.FirstOrDefaultAsync(org => org.Id == legionMember.OrganizationId);
                if (organization == null)
                    return $"Could not find an organization with Id {legionMember.OrganizationId}.";


                // Kick the target organization.
                context.LegionMembers.Remove(legionMember);

                // Save changes to database.
                await context.SaveChangesAsync();


                // Return the succes message.
                return $"The \"{organization.Name}\" organization has been kicked out of the \"{legion.Name}\" legion.";
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
                .WithDescription("Allows a legion leader to kick an organization out of the legion.")
                .AddOption("id", ApplicationCommandOptionType.Integer, "The organization that will be kicked out", isRequired: true)
                .Build();
        }
    }
}
