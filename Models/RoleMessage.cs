using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBot.Models
{
    public class RoleMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong RoleID { get; set; }
        [Required]
        public string RoleName { get; set; }
        [Required]
        public string Message { get; set; }

        [ForeignKey("Guild")]
        public ulong GuildID { get; set; }
        public Guild Guild { get; set; }
    }
}
