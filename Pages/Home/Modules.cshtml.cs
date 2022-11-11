using DiscordBot.Data;
using DiscordBot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscordBot.Pages.Home
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

            new Module() { Name = "Role Messages", Description = "Configure What messages to send when a user gets a role.", PageName = "RoleMessages" }
        };
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? PageName { get; set; }
    }
}
