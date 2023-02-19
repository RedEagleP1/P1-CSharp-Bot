using Models;
namespace WebApp.Pages.Modules.RoleMessages
{
    public class RolesDropdownPartialModel
    {
        public string ButtonName { get; set; }
        public IEnumerable<Role> Roles { get; set; }
        public bool AddNoneOption { get; set; }
    }
}
