using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
	public class Legion
	{
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong Id { get; set; }

        [Required]
        public ulong LeaderID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int MaxMembers { get; set; }
        [Required]
        public ulong GuildId { get; set; }
    }
}
