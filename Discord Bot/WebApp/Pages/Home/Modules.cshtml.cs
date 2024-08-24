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
        public static List<Module> Modules = new List<Module>() 
        {
            new Module() {Name = "Automations", Description = "Set up automations for the server.", PageName="Automations"},
            new Module() {Name = "Currency Resets", Description = "Reset specific or all currencies for all users in a guild.", PageName="CurrencyResets"},
            new Module() {Name = "Display Money", Description = "Look at how much currency everyone on the server has.", PageName="DisplayMoney"},
            new Module() {Name = "Global Rewards", Description = "Set amount/currency gained for ALL voice channels.", PageName="GlobalRewards"},
            new Module() {Name = "Role Costs And Rewards", Description = "Set the cost and reward of each role.", PageName="RoleCostsAndRewards"},
            new Module() {Name = "Role Message And Survey Repeats", Description = "Set whether to repeat the message and surveys of a role.", PageName="RoleMessageAndSurveyRepeats"},
            new Module() {Name = "Role Messages", Description = "Configure What messages to send when a user gets a role.", PageName = "RoleMessages" },
            new Module() {Name = "Role Surveys", Description = "Configure what survey to send when a user gets a role.", PageName = "RoleSurveys"},
            new Module() {Name = "Roles For Sale", Description = "Set which roles are available for sale", PageName = "RolesForSale"},
            new Module() {Name = "Team Settings", Description = "Set team settings such as member caps", PageName = "TeamSettings"},
            new Module() {Name = "Text Channel Message Validation", Description = "Set message validation on specific channels", PageName = "TextChannelMessageValidation" },
            new Module() {Name = "Global Rewards", Description = "Set amount/currency gained for ALL voice channels.", PageName="GlobalRewards"},
            new Module() {Name = "Currency Resets", Description = "Reset specific or all currencies for all users in a guild.", PageName="CurrencyResets"},
            new Module() {Name = "Automations", Description = "Set up automations for the server.", PageName="Automations"},
			new Module() {Name = "Voice Channel Currency Gain", Description = "Set amount/currency gained for voice channels", PageName = "VoiceChannelCurrencyGains"},
			new Module() {Name = "Shop", Description = "Set up shop items for the server.", PageName="Shop"}
		};
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? PageName { get; set; }
    }
}
