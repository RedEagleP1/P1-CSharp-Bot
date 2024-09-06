using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.SlashCommands.DbUtils
{
    /// <summary>
    /// This class contains some useful methods for getting user data from the database.
    /// Because it accesses the database, its methods should be called in between calls to
    /// await DBReadWrite.LockReadWrite();
    /// and
    /// DBReadWrite.ReleaseLock();
    /// See the organization and legion commands for examples.

    /// </summary>
    internal static class UserDataUtils
    {
        /// <summary>
        /// Checks if the user is in an organization.
        /// </summary>
        /// <param name="userId">The Id of the user to check.</param>
        /// <param name="context">The database context.</param>
        /// <returns>null if the user is not in an organization, or their organization membership record.</returns>
        public static async Task<OrganizationMember?> CheckIfUserIsInAnOrg(ulong userId, ApplicationDbContext context)
        {
            if (context.OrganizationMembers.Count() < 1)
                return null;

            OrganizationMember? orgMember = context.OrganizationMembers.Count() > 0 ? await context.OrganizationMembers.FirstOrDefaultAsync(x => x.UserId == userId)
                                                                                    : null;
            if (orgMember != null)
                return orgMember;
            else
                return null;
        }

        /// <summary>
        /// Checks if the user is in a legion.
        /// </summary>
        /// <param name="userId">The Id of the user to check.</param>
        /// <param name="context">The database context.</param>
        /// <returns>null if the user is not in a legion, or membership record of their organization in the legion.</returns>
        public static async Task<LegionMember?> CheckIfUserIsInALegion(ulong userId, ApplicationDbContext context)
        {
            if (context.LegionMembers.Count() < 1)
                return null;


            OrganizationMember? orgMember = context.OrganizationMembers.Count() > 0 ? await context.OrganizationMembers.FirstOrDefaultAsync(x => x.UserId == userId)
                                                                                    : null;
            if (orgMember == null)
                return null;


            LegionMember? legionMember = context.LegionMembers.Count() > 0 ? await context.LegionMembers.FirstOrDefaultAsync(x => x.OrganizationId == orgMember.OrganizationId)
                                                                           : null;
            if (legionMember == null)
                return null;
            else
                return legionMember;
        }

        /// <summary>
        /// Checks if the user is the leader of a legion.
        /// </summary>
        /// <param name="userId">The Id of the user to check.</param>
        /// <param name="context">The database context.</param>
        /// <returns>null if the user is not in an legion, or the id of the legion they are in.</returns>
        public static async Task<Legion?> CheckIfUserIsALegionLeader(ulong userId, ApplicationDbContext context)
        {
            if (context.Legions.Count() < 1)
                return null;


            OrganizationMember? orgMember = context.OrganizationMembers.Count() > 0 ? await context.OrganizationMembers.FirstOrDefaultAsync(x => x.UserId == userId)
                                                                                    : null;
            if (orgMember == null)
                return null;


            Legion? legion = context.Legions.Count() > 0 ? await context.Legions.FirstOrDefaultAsync(x => x.LeaderID == userId)
                                                         : null;
            if (legion == null)
                return null;
            else
                return legion;
        }

        /// <summary>
        /// Checks if the user is the leader of an organization.
        /// </summary>
        /// <param name="userId">The Id of the user to check.</param>
        /// <param name="context">The database context.</param>
        /// <returns>null if the user is not a legion leader, or the id of the legion they are the leader of.</returns>
        public static async Task<Organization?> CheckIfUserIsAnOrgLeader(ulong userId, ApplicationDbContext context)
        {
            if (context.Organizations.Count() < 1)
                return null;


            Organization? organization = context.Organizations.Count() > 0 ? await context.Organizations.FirstOrDefaultAsync(x => x.LeaderID == userId)
                                                                           : null;
            if (organization == null)
                return null;
            else
                return organization;
        }

    }
}
