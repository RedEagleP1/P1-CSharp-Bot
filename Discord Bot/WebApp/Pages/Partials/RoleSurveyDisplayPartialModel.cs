using Models;
using Models.HelperClasses;

namespace WebApp.Pages.Partials
{
    public class RoleSurveyDisplayPartialModel
    {
        public bool DisplayButtons { get; set; }
        public bool CanHaveCondition { get; set; }
        public RoleSurvey_HM RoleSurvey_HM { get; set; }
    }
}
