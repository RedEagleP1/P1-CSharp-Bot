using Models.HelperClasses;

namespace WebApp.Pages.Partials
{
	public class RoleMessageAndSurveyRepeatRowPartialModel
	{
        public RoleMessageAndSurveyRepeat_HM RoleMessageAndSurveyRepeat_HM { get; set; }
        public ulong GuildId { get; set; }
        public bool IsEditMode { get; set; }
    }
}
