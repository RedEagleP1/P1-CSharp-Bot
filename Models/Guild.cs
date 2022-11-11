using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBot.Models
{
    public class Guild
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong ID { get; set; }
        [Required]
        public string Name { get; set; }
        public List<RoleMessage> RoleMessages { get; set; } = new List<RoleMessage>();
    }
}
