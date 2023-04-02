using Models;
using Models.HelperClasses;

namespace WebApp.Pages.Partials
{
    public class RoleSurveyTreeBranchPartialModel
    {
        public bool IsEditMode { get; set; }
        public int RoleSurveyEditId { get; set; }
        public RoleSurvey_HM CurrentRoleSurvey_HM { get; set; }
        public List<RoleSurvey_HM> AllRoleSurvey_HMs { get; set; }
        public List<Role> AllRoles { get; set; }
        public bool CanHaveCondition { get; set; }
    }
}
