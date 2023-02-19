using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Home
{
    [Authorize(Policy = "Allowed")]
    public class ModulesModel : PageModel
    {
        public ulong GuildID { get; set; }
        public string GuildName { get; set; }
        public List<Module> Modules { get => Module.Modules; }  
        public void OnGet(ulong id, string name)
        {
            GuildID = id;
            GuildName = name;
        }        
    }

    public class Module
    {
        public static List<Module> Modules = new List<Module>() {

            new Module() { Name = "Role Messages", Description = "Configure What messages to send when a user gets a role.", PageName = "RoleMessages" },
            new Module() {Name = "Role Costs", Description = "Set the cost of each role.", PageName="RoleCosts"}
        };
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? PageName { get; set; }
    }
}
