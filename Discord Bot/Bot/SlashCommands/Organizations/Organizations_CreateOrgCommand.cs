using Bot.SlashCommands.DbUtils;
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
                using var context = DBContextFactory.GetNewContext();

                OrganizationMember? memberTest = await UserDataUtils.CheckIfUserIsInAnOrg(command.User.Id, context);
                if (!Organization.ALLOW_USER_TO_JOIN_MULTIPLE_ORGS && memberTest != null)
                    return "You are already in an organization so you cannot create a new one.";


                // Try to get the name option.
                SocketSlashCommandDataOption? nameOption = command.Data.Options.FirstOrDefault(x => x.Name == "name");
                if (nameOption == null)
                    return "Please provide a name for the new organization.";

                // Check if the name option contains a valid value.
                string? orgName = nameOption.Value.ToString();
                if (orgName == null || orgName.Length < Organization.DEFAULT_MIN_ORG_NAME_LENGTH)
                    return $"Please provide a name that is at least {Organization.DEFAULT_MIN_ORG_NAME_LENGTH} characters long for the new organization.";

                orgName = orgName.Trim();
            

                // Check if there is already an organization with this name.
                Organization? org = context.Organizations.Count() > 0 ? await context.Organizations.AsNoTracking().FirstOrDefaultAsync(x => x.Name == orgName)
                                                                      : null;
                if (org != null)
                    return "An organization with this name already exists.";


                // Get the guild Id.
                ulong guildId = 0;
                if (command.GuildId == null)
                    return $"Failed to create new organization \"{orgName}\". GuildId is null.";
                else
                    guildId = (ulong) command.GuildId;


                // Get the relevant team settings record.
                TeamSettings? teamSettings = TeamSettingsUtils.GetTeamSettingsForGuild(guildId, context);
                bool createdNewTeamSettings = false;
                if (teamSettings == null)
                {
                    createdNewTeamSettings = true;
                    teamSettings = TeamSettings.CreateDefault(guildId);
                }

                // First, add the new organization to the orgs table.
                Organization newOrg = new Organization();
                newOrg.Name = orgName.Trim();
                newOrg.LeaderID = command.User.Id;
                newOrg.CurrencyId = Organization.DEFAULT_CURRENCY_ID;
                newOrg.GuildId = guildId;
                context.Organizations.Add(newOrg);

                // If we created a new team settings record, then add it to the database.
                if (createdNewTeamSettings)
                    context.TeamSettings.Add(teamSettings);

                // Save changes to the database.
                await context.SaveChangesAsync();


                // Get the id of the new org.
                Organization? result = context.Organizations.Count() > 0 ? await context.Organizations.FirstOrDefaultAsync(x => x.Name == orgName)
                                                                        : null;                                                                           
                if (result == null)
                    return $"Failed to create new organization \"{orgName}\".";

                // Now, add an entry to the org members table.
                OrganizationMember member = new OrganizationMember();
                member.OrganizationId = result.Id;
                member.UserId = command.User.Id;
                context.OrganizationMembers.Add(member);

                // Save changes to database.
                await context.SaveChangesAsync();

                return $"Created the new organization \"{orgName}\", with you as the leader.";

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
                .WithDescription("Creates a new organization with you as its leader")
                .AddOption("name", ApplicationCommandOptionType.String, "The name of the new organization", true)
                .Build();
        }
    }
}
