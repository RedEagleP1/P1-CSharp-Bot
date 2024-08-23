using Bot.SlashCommands.DbUtils;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Text;

namespace Bot.SlashCommands.Organizations
{
    /// <summary>
    /// This command displays information about the specified organization.
    /// If no org Id is supplied, this command will display information about the user's organization if they are in one.
    /// </summary>
    internal class Organizations_OrgInfoCommand : ISlashCommand
    {
        const string name = "org_info";
        readonly SlashCommandProperties properties = CreateNewProperties();

        private DiscordSocketClient client;

        public string Name => name;
        public SlashCommandProperties Properties => properties;


        public Organizations_OrgInfoCommand(DiscordSocketClient client)
        {
            this.client = client;
        }

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
            // First, find the user.
            var user = client.GetGuild(Settings.P1RepublicGuildId)?.GetUser(command.User.Id);
            if (user == null)
                return "Could not find user info.";


            await DBReadWrite.LockReadWrite();
            try
            {

                using var context = DBContextFactory.GetNewContext();

                // Try to get the id option.
                SocketSlashCommandDataOption? idOption = command.Data.Options.FirstOrDefault(x => x.Name == "id");
                ulong orgId = 0;
                if (idOption == null)
                {
                    // No id was provided, so we need to get the Id of the organization the user is in.
                    OrganizationMember? member = context.OrganizationMembers.Count() > 0 ? await context.OrganizationMembers.FirstOrDefaultAsync(x => x.UserId == command.User.Id)
                                                                                         : null;
                    if (member == null)
                        return "You cannot view info on your own organization since you're not in one. Use this command again, and provide the Id of the organization you want to view info for.";

                    orgId = member.OrganizationId;
                }
                else
                {
                    // This double cast looks silly, but when I casted directly to ulong it kept crashing with an invalid cast error for some reason.
                    orgId = (ulong)(long)idOption.Value;
                }


                // Check if there is an organization with this Id.
                Organization? org = context.Organizations.Count() > 0 ? await context.Organizations.FirstOrDefaultAsync(x => x.Id == orgId)
                                                                      : null;
                if (org == null)
                    return $"There is no organization with Id {orgId}.";


                // Find the organization leader
                SocketUser leader = client.GetUser(org.LeaderID);



				// Get the relevant team settings record.
				TeamSettings? teamSettings = TeamSettingsUtils.GetTeamSettingsForGuild(org.GuildId, context);
				if (teamSettings == null)
				{
					teamSettings = TeamSettings.CreateDefault(org.GuildId);

					// Add the new team settings record into the database.
					context.TeamSettings.Add(teamSettings);
					await context.SaveChangesAsync();
				}


				// Get the members of the organization
				List<OrganizationMember>? members = context.OrganizationMembers.Count() > 0 ? await context.OrganizationMembers.Where(x => x.OrganizationId == orgId).ToListAsync()
                                                                                            : null;
                int memberCount = members != null ? members.Count : 0;

                // Fill out the EmbedBuilder.
                EmbedBuilder embedBuilder = new EmbedBuilder()
                    .WithAuthor(command.User.Username, command.User.GetAvatarUrl() ?? command.User.GetDefaultAvatarUrl())
                    .WithTitle("Organization Information")                    
                    .WithDescription($"**Name:** {org.Name} \n " +
                                     $"**ID:** {org.Id} \n " +
                                     $"**Treasury Amount:** {org.TreasuryAmount} \n " +
                                     $"**Leader:** {leader.Username} \n " +
                                     $"**Members Count:** {memberCount} / {teamSettings.MaxMembersPerOrg}")
                    .AddField("Members:", OrgDataUtils.GetMemberPingsList(command.User.Id, client, members)) // This adds the members list into the embed.
                    .WithColor(Color.Blue)
                    .WithCurrentTimestamp();


                // Insert the embed into the response
                await command.ModifyOriginalResponseAsync(response =>
                {
                    response.Content = "";
                    response.Embed = embedBuilder.Build();
                    response.Flags = MessageFlags.Ephemeral;
                });


                // This causes the message content to be set to null. We don't need it since we are using an embed for the content.
                return "";
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
                .WithDescription("Displays information about the organization with the specified Id, or the user's organization.")
                .AddOption("id", ApplicationCommandOptionType.Integer, "The Id of the organization to display information about", false)
                .Build();
        }
    }
}
