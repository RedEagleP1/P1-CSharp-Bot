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
    public class LegionDataUtils
    {
        /// <summary>
        /// Retrieves the legion of a specified legion leader.
        /// </summary>
        /// <param name="leaderId">The Id of the legion leader whose legion is to be retrieved.</param>
        /// <param name="context">The database context</param>
        /// <returns>The legion of the specified legion leader, or null if the legion was not found.</returns>
        public static async Task<Legion?> GetLegionFromLeaderId(ulong leaderId, ApplicationDbContext context)
        {
            // Find the legion.
            Legion? legion = await context.Legions.FirstAsync(lg => lg.LeaderID == leaderId);
            if (legion == null)
            {
                Console.WriteLine($"ERROR: Could not find the legion with LeaderId={leaderId}!");
                return null;
            }


            // Return the legion.
            return legion;
        }

        /// <summary>
        /// Extracts the user Id from a legion join request message's content.
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
                Console.WriteLine($"ERROR: An error occurred while trying to get the user Id from the legion join request.n\"{ex.Message}\"\n    Inner Exception: \"{(ex.InnerException != null ? ex.InnerException.Message : "")}\"");

                return false;
            }
        }

        /// <summary>
        /// This function creates the join request we will send to the legions's leader.
        /// </summary>
        /// <returns>The join request message to send to the legion leader.</returns>
        public static MessageComponent CreateJoinRequest()
        {
            // Build the join request we will send to the team lead
            ActionRowBuilder row1Builder = new ActionRowBuilder()
                .WithButton("Accept", "accept_join_legion", ButtonStyle.Primary)
                .WithButton("Deny", "deny_join_legion", ButtonStyle.Secondary);

            ComponentBuilder components = new ComponentBuilder()
                .WithRows(new List<ActionRowBuilder>() { row1Builder });

            return components.Build();
        }
    }
}
