using Bot.SlashCommands.DbUtils;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;

namespace Bot.SlashCommands.Organizations
{
    /// <summary>
    /// This command displays information about the specified legion.
    /// If no legion Id is supplied, this command will display information about the user's legion if they are in one.
    /// </summary>
    internal class Legions_LegionInfoCommand : ISlashCommand
    {
        const string name = "legion_info";
        readonly SlashCommandProperties properties = CreateNewProperties();

        private DiscordSocketClient client;

        private int currentMembers = 0;
        private int maxMembers = 0;
        private float treasuriesTotal = 0f;

        public string Name => name;
        public SlashCommandProperties Properties => properties;


        public Legions_LegionInfoCommand(DiscordSocketClient client)
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
                ulong legionId = 0;
                if (idOption == null)
                {
                    // No id was provided, so we need to get the Id of the legion the user is in.
                    LegionMember? member = await UserDataUtils.CheckIfUserIsInALegion(user.Id, context);
                    if (member == null)
                        return "You cannot view info on your own legion since you're not in one. Use this command again, and provide the Id of the organization you want to view info for.";

                    legionId = member.LegionId;
                }
                else
                {
                    // This double cast looks silly, but when I casted directly to ulong it kept crashing with an invalid cast error for some reason.
                    legionId = (ulong)(long)idOption.Value;
                }


                // Check if there is an legion with this Id.
                Legion? legion = context.Legions.Count() > 0 ? await context.Legions.FirstOrDefaultAsync(x => x.Id == legionId)
                                                          : null;
                if (legion == null)
                    return $"There is no legion with Id {legionId}.";


                // Find the legion leader
                SocketUser leader = client.GetUser(legion.LeaderID);

                // Get the members of the legion
                List<LegionMember>? members = context.LegionMembers.Count() > 0 ? await context.LegionMembers.Where(x => x.LegionId == legionId).ToListAsync()
                                                                                            : null;
                int memberCount = members != null ? members.Count : 0;


                List<LegionMemberInfo> orgDescriptions = await GetOrgData(members, context);


                // Fill out the EmbedBuilder.
                EmbedBuilder embedBuilder = new EmbedBuilder()
                    .WithAuthor(command.User.Username, command.User.GetAvatarUrl() ?? command.User.GetDefaultAvatarUrl())
                    .WithTitle("Legion Information")
                    .WithDescription($"**Name:** {legion.Name} \n " +
                                     $"**ID:** {legion.Id} \n " +
                                     $"**Treasuries Total:** {treasuriesTotal} \n " +
                                     $"**Leader:** <@{leader.Id}> \n " +
                                     $"**Organizations Count:** {memberCount} / {legion.MaxMembers} \n" +
                                     $"**Members Count:** {currentMembers} / {maxMembers} \n _ \n")
                    .WithColor(Color.Blue)
                    .WithCurrentTimestamp();

                foreach (var orgInfo in orgDescriptions)
                {
                    embedBuilder.AddField($"**__Child Organization:__**  {orgInfo.OrgName}", orgInfo.OrgDescription);
                }
                    


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

        private async Task<List<LegionMemberInfo>> GetOrgData(List<LegionMember>? orgs, ApplicationDbContext context)
        {
            List<LegionMemberInfo> orgInfos = new();


            currentMembers = 0;
            maxMembers = 0;
            treasuriesTotal = 0;


            if (orgs == null || orgs.Count < 1)
                return orgInfos;


            int i = 0;
            foreach (var member in orgs)
            {
                // Get the organization.
                Organization? org = context.Organizations.Count() > 0 ? context.Organizations.FirstOrDefault(x => x.Id == member.OrganizationId)
                                                                      : null;
                if (org == null)
                {
                    Console.WriteLine($"ERROR: The Legion_Info command could not find an org with Id {member.OrganizationId} while trying to display its description. Skipping it.");
                    continue;
                }


                // Get the members of the organization
                List<OrganizationMember>? orgMembers = context.OrganizationMembers.Count() > 0 ? await context.OrganizationMembers.Where(x => x.OrganizationId == org.Id).ToListAsync()
                                                                                               : null;
                int memberCount = orgs != null ? orgs.Count : 0;


                string teamLead = client.GetUser(org.LeaderID).Username;

                string description = $"**ID:** {org.Id} \n " +
                                     $"**Treasury Amount:** {org.TreasuryAmount} \n " +
                                     $"**Leader:** <@{org.LeaderID}> \n " +
                                     $"**Members Count:** {memberCount} / {org.MaxMembers} \n";



                LegionMemberInfo orgInfo = new();
                orgInfo.OrgName = org.Name;
                orgInfo.OrgDescription = description;
                treasuriesTotal += org.TreasuryAmount;
                maxMembers += org.MaxMembers;

                // Get the Ids of all members of this organization.
                List<string> memberNames = new();
                foreach (OrganizationMember orgMember in orgMembers)
                    memberNames.Add("<@" + orgMember.UserId + ">\n");

                // Sort the list.
                memberNames.Sort();

                // Add the members' names to the org description string.
                StringBuilder b = new();
                int j = 0;
                foreach (OrganizationMember orgMember in orgMembers)
                {
                    if (orgMember != null)
                    {
                        orgInfo.MemberIds.Add(orgMember.Id);
                        currentMembers++;

                        b.Append(memberNames[j]);

                        j++;
                    }
                   
                } // end foreach

                orgInfo.OrgDescription += b.ToString() + "_\n";

                orgInfos.Add(orgInfo);

                i++;

            } // end foreach member


            return orgInfos;
        }

        static SlashCommandProperties CreateNewProperties()
        {
            return new SlashCommandBuilder()
                .WithName(name)
                .WithDescription("Displays information about the legion with the specified Id, or the user's legion.")
                .AddOption("id", ApplicationCommandOptionType.Integer, "The Id of the organization to display information about", false)
                .Build();
        }


        public class LegionMemberInfo
        {
            public string OrgName = string.Empty;
            public string OrgDescription = string.Empty;
            public List<ulong> MemberIds = new List<ulong>();
        }
    }
}
