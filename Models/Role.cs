using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
	public class Role
	{
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Message { get; set; }
        public float Cost { get; set; }
        public bool HasSurvey { get; set; }
        public List<RoleSurvey> RoleSurveys { get; set; }
        public List<RoleSurveyOption> OptionsThatHaveThisRole { get; set; }
        //Relations
        public ulong GuildId { get; set; }
        public Guild Guild { get; set; }

    }
}
