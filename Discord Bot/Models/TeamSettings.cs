using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class TeamSettings
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong Id { get; set; }

        [Required]
        public ulong GuildId { get; set; }
        [Required]
        public int MaxOrgsPerLegion { get; set; }
        [Required]
        public int MaxMembersPerOrg { get; set; }



        /// <summary>
        /// This function creates a TeamSettings object filled with default values.
        /// </summary>
        /// <param name="guildId">The Id of the guild this TeamSettings object will apply to.</param>
        /// <returns>A TeamSettings object filled with default values.</returns>
        public static TeamSettings CreateDefault(ulong guildId)
        {
            TeamSettings settings = new TeamSettings()
            {
                GuildId = guildId,
                MaxOrgsPerLegion = Legion.DEFAULT_MAX_ORGS,
                MaxMembersPerOrg = Organization.DEFAULT_MAX_MEMBERS,
            };

            return settings;
        }
    }
}
