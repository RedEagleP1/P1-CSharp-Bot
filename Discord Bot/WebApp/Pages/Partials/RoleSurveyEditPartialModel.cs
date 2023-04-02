using Models;
using Models.HelperClasses;

namespace WebApp.Pages.Partials
{
    public class RoleSurveyEditPartialModel
    {
        public bool CanHaveCondition { get; set; }
        public List<RoleSurveyOption> AvailableTriggerOptions { get; set; }
        public List<Role> AllRoles { get; set; }
        public RoleSurvey_HM RoleSurvey_HM { get; set; }
    }
}
