using Bot.SlashCommands.DbUtils;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Bot.SlashCommands.Organizations
{
    /// <summary>
    /// This command creates a new legion.
    /// The user who invoked the command will automatically be set as the legion's leader.
    /// </summary>
    internal class Legions_CreateLegionCommand : ISlashCommand
    {
        const string name = "create_legion";
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

                // Check if the user is already a legion leader.
                if (!OrganizationConstants.ALLOW_USER_TO_JOIN_MULTIPLE_ORGS)
                {
                    Legion? tempLegion = UserDataUtils.CheckIfUserIsALegionLeader(command.User.Id, context).Result;
                    if (tempLegion != null)
                        return "You are already a legion leader, so you cannot create a new one.";
                }

                // Check if the user is an organization leader.
                Organization? org = UserDataUtils.CheckIfUserIsAnOrgLeader(command.User.Id, context).Result;
                if (org == null)
                    return "You are not an organization leader, so you cannot create a new legion.";

                // Try to get the name option.
                SocketSlashCommandDataOption nameOption = command.Data.Options.First(x => x.Name == "name");
                if (nameOption == null)
                {
                    return "Please provide a name for the new legion.";
                }

                // Check if the name option contains a valid value.
                string? legionName = nameOption.Value.ToString();
                if (legionName == null || legionName.Length < OrganizationConstants.MIN_ORG_NAME_LENGTH)
                {
                    return $"Please provide a name that is at least {OrganizationConstants.MIN_ORG_NAME_LENGTH} characters long for the new legion.";
                }
                legionName = legionName.Trim();
            
                // Check if there is already an legion with this name.
                Legion? tempLegion2 = context.Legions.Count() > 0 ? await context.Legions.AsNoTracking().FirstAsync(x => x.Name == legionName)
                                                                  : null;
                if (tempLegion2 != null)
                {
                    return "A legion with this name already exists.";
                }


                // Get the guild Id.
                ulong guildId = 0;
                if (command.GuildId == null)
                    return $"Failed to create new legion \"{legionName}\". GuildId is null.";
                else
                    guildId = (ulong) command.GuildId;

             
                // First, add the new legion to the legions table.
                Legion newLegion = new Legion();
                newLegion.Name = legionName.Trim();
                newLegion.LeaderID = command.User.Id;
                newLegion.GuildId = guildId;
                newLegion.MaxMembers = OrganizationConstants.MAX_ORG_MEMBERS;
                context.Legions.Add(newLegion);

                await context.SaveChangesAsync();


                // Get the id of the new legion.
                Legion? legion = context.Legions.Count() > 0 ? await context.Legions.FirstAsync(x => x.Name == legionName)
                                                             : null;
                if (legion == null)
                    return $"Failed to create new legion \"{legionName}\".";

                // Now, add an entry to the legion members table.
                LegionMember member = new LegionMember();
                member.OrganizationId = org.Id;
                member.LegionId = legion.Id;
                context.LegionMembers.Add(member);

                // Save changes to database.
                await context.SaveChangesAsync();

                return $"Created the new legion \"{legionName}\", with you as the leader.";

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
                .WithDescription("Creates a new legion with you as its leader")
                .AddOption("name", ApplicationCommandOptionType.String, "The name of the new legion", true)
                .Build();
        }
    }
}
