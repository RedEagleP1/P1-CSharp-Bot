using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
	public class OrganizationMember
	{
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong Id { get; set; }
        [Required]
        public ulong UserId { get; set; }       
        [Required]
        [ForeignKey("OrganizationId")]
        public ulong OrganizationId { get; set; }

    }
}
