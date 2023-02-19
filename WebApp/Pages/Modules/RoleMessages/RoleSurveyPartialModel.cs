using Models;

namespace WebApp.Pages.Modules.RoleMessages
{
    public class RoleSurveyPartialModel
    {
        public bool CanHaveCondition { get; set; }
        public List<RoleSurveyOption> AvailableTriggerOptions { get; set; }
        public List<Role> AllRoles { get; set; }
        public RoleSurvey RoleSurvey { get; set; }
    }
}
