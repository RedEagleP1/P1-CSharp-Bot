using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.SlashCommands.Organizations
{
    internal static class LegionConstants
    {
        internal const bool ALLOW_ORG_TO_JOIN_MULTIPLE_LEGIONS = false; // I added this option mostly for easy debugging.
        internal const int MAX_ORG_MEMBERS = 10; // The max number of organizations that a legion can have.
        internal const int MIN_ORG_NAME_LENGTH = 3; // The minimum length that a legion name must be.
        internal const string MODERATOR_ROLE = "Admin"; // This is the role that is allowed to delete organizations.
    }
}
