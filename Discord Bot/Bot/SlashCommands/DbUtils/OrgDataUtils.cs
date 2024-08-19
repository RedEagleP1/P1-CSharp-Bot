using Discord;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.SlashCommands.DbUtils
{
    static public class OrgDataUtils
    {
        /// <summary>
        /// Builds a string containing a ping list of all members in the organization.
        /// </summary>
        /// <param name="userId">The Id of the user who invoked the command that called this function.</param>
        /// <param name="members">A list of all the organization's members.</param>
        /// <param name="excludeUser">Whether or not to exclude the user that invoked the command that calle this function. For example, this is set to true when this function is called from the ping command.</param>
        /// <param name="alphabetized">Whether or not the returned list should be alphabetized.</param>
        /// <returns>A string containing a ping list of all members in the organization or null if the organization is not found or has no members.</returns>
        public static string GetMemberPingsList(ulong userId, List<OrganizationMember>? members, bool excludeUser = false, bool alphabetized = true)
        {
            if (members == null || members.Count < 1)
                return "[No Members]";


            List<string> memberNames = new();


            // Get the names of all members of the organization.
            foreach (OrganizationMember orgMember in members)
                memberNames.Add("<@" + orgMember.UserId + ">\n");

            // Sort the list.
            if (alphabetized)
                memberNames.Sort();

            // Add the members' names to the org description string.
            StringBuilder b = new();
            int j = 0;
            foreach (OrganizationMember orgMember in members)
            {
                if (orgMember != null)
                {
                    b.Append(memberNames[j]);

                    j++;
                }
            } // end foreach


            // If the string builder is still empty, we need to append something. Otherwise, Discord will hang if a null, empty or whitespace string
            // is returned in the response.Contents field, or in certain other fields like the value property of a field. The bot program keeps
            // running, but the bot will just say "Thinking..." forever on that message.
            if (b.Length < 1)
            {
                // NOTE: "\u200B" is a unicode character that acts just like a space, but does not count as a whitespace character.
                b.Append("\u200B");
            }

            return b.ToString();
        }


        /// <summary>
        /// Retrieves the organization of a specified team lead.
        /// </summary>
        /// <param name="leaderId">The Id of the team lead whose organization is to be retrieved.</param>
        /// <param name="context">The database context</param>
        /// <returns>The organization of the specified team lead, or a dummy Organization object with an error message in the name field and -1 in the MaxMembers field.</returns>
        public static async Task<Organization> GetOrgFromLeaderId(ulong leaderId, ApplicationDbContext context)
        {
            // Find the organization.
            Organization? org = await context.Organizations.FirstAsync(o => o.LeaderID == leaderId);
            if (org == null)
            {
                Console.WriteLine($"ERROR: Could not find the organization with LeaderId={leaderId}!");
                return new Organization() 
                { 
                    Name = "[ERROR: COULD NOT FIND ORGANIZATION!]",
                    MaxMembers = -1,
                };
            }


            // Return the organization.
            return org;
        }

        /// <summary>
        /// Extracts the user Id from an organization join request message's content.
        /// </summary>
        /// <param name="msgContent">The value from the message's content field.</param>
        /// <param name="senderId">The extracted user Id.</param>
        /// <returns>True if the user Id was extracted successfully or false otherwise.</returns>
        public static bool ExtractSenderIdFromMessageContent(string msgContent, out ulong? senderId)
        {
            senderId = null;

            try
            {
                int firstSpace = msgContent.IndexOf(" ");

                string temp = msgContent.Substring(0, firstSpace - 1);

                // Trim off the leading "<@" and trailing ">"
                temp = temp.Substring(2, temp.Length - 2);

                senderId = ulong.Parse(temp);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: An error occurred while trying to get the user Id from the organization join request.n\"{ex.Message}\"\n    Inner Exception: \"{(ex.InnerException != null ? ex.InnerException.Message : "")}\"");

                return false;
            }
        }

        /// <summary>
        /// This function creates the join request we will send to the organization's team lead.
        /// </summary>
        /// <returns>The join request message to send to the team lead.</returns>
        public static MessageComponent CreateJoinRequest()
        {
            // Build the join request we will send to the team lead
            ActionRowBuilder row1Builder = new ActionRowBuilder()
                .WithButton("Accept", "accept_join_org", ButtonStyle.Primary)
                .WithButton("Deny", "deny_join_org", ButtonStyle.Secondary);

            ComponentBuilder components = new ComponentBuilder()
                .WithRows(new List<ActionRowBuilder>() { row1Builder });

            return components.Build();
        }

    }
}
