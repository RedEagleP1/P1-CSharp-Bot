using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.SlashCommands.Organizations
{
    internal static class OrganizationConstants
    {
        internal const bool ALLOW_USER_TO_JOIN_MULTIPLE_ORGS = false; // I added this option mostly for easy debugging.
        internal const int CURRENCY_ID = 2; // Defines which type of currency organziations will take.
        internal const int MAX_ORG_MEMBERS = 10; // The max number of members that an organization can have.
        internal const int MIN_ORG_NAME_LENGTH = 3; // The minimum length that an organization name must be.
        internal const float MIN_DONATION_AMOUNT = 1f; // The minimum amount that a user can donate to their organization
        internal const string MODERATOR_ROLE = "Admin"; // This is the role that is allowed to delete organizations.
    }
}
