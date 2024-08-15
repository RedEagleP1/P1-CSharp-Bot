using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
	public class LegionMember
	{
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DefaultValue(1)]
        public ulong Id { get; set; }
     
        [Required]
        [ForeignKey("OrganizationId")]
        public ulong OrganizationId { get; set; }

        [Required]
        public ulong LegionId { get; set; }

    }
}
