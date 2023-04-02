using Models;

namespace WebApp.Pages.Partials
{
    public class RoleSurveyTriggersDropdownPartialModel
    {
        public string ButtonName { get; set; }
        public IEnumerable<RoleSurveyOption> Triggers { get; set; }
    }
}
