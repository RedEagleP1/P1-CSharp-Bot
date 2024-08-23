using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
	public class Legion
	{
		public const bool ALLOW_ORG_TO_JOIN_MULTIPLE_LEGIONS = false; // I added this option mostly for easy debugging.
		public const int DEFAULT_MAX_ORGS = 13; // The max number of organizations that a legion can have.
		public const int DEFAULT_MIN_NAME_LENGTH = 3; // The minimum length that a legion name must be.
		public const string MODERATOR_ROLE = "Admin"; // This is the role that is allowed to delete legions.



		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong Id { get; set; }

        [Required]
        public ulong LeaderID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public ulong GuildId { get; set; }
    }
}
