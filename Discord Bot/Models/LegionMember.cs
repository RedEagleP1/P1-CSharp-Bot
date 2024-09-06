using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
	public class LegionMember
	{
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong Id { get; set; }
     
        [Required]
        public ulong OrganizationId { get; set; }

        [Required]
        public ulong LegionId { get; set; }

    }
}
