using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
	public class Organization
	{
		public const bool ALLOW_USER_TO_JOIN_MULTIPLE_ORGS = false; // I added this option mostly for easy debugging.
		public const int DEFAULT_CURRENCY_ID = 2; // Defines which type of currency organziations will take.
		public const int DEFAULT_MAX_MEMBERS = 14; // The max number of members that an organization can have.
		public const int DEFAULT_MIN_ORG_NAME_LENGTH = 3; // The minimum length that an organization name must be.
		public const float DEFAULT_MIN_DONATION_AMOUNT = 1f; // The minimum amount that a user can donate to their organization
		public const string MODERATOR_ROLE = "Admin"; // This is the role that is allowed to delete organizations.



		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong Id { get; set; }

        [Required]
        public ulong LeaderID { get; set; }
        [Required]
        public string Name { get; set; }
        public ulong TreasuryAmount { get; set; }
        [Required]
        public ulong CurrencyId { get; set; }
        [Required]
        public ulong GuildId { get; set; }
    }
}
