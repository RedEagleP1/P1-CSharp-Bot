using Models;
using Models.HelperClasses;

namespace WebApp.Pages.Partials
{
    public class RoleMessageRowPartialModel
    {
        public RoleMessage_HM RoleMessage_HM { get; set; }
        public ulong GuildId { get; set; }
        public bool IsEditMode { get; set; }
    }
}
