using Discord;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Diagnostics;
using WebApp.Pages.Partials;

namespace WebApp.Pages.Modules.Automations
{
    [Authorize(Policy = "Allowed")]
    public class IndexModel : PageModel
    {
        public Guild Guild { get; set; }
        public List<AutomationInfo> WhenAutomations { get; set; }
        public List<AutomationInfo> DoAutomations { get; set; }
        public List<AutomationInfo> IfAutomations { get; set; }

        public List<Automation> Automations { get; set; }
		public List<AutomationInfo> AutomationInfos { get; set; }

		private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task OnGet(ulong guildId)
        {
            Guild = await _db.Guilds.FirstOrDefaultAsync(g => g.Id == guildId);
            var dropModel = new AutomationDropdownModel();

            //Add Lists
            WhenAutomations = dropModel.con_When.ToList();
            DoAutomations = dropModel.con_Do.ToList();
            IfAutomations = dropModel.con_If.ToList();

			//Get Info
			List<Automation> AutomationList = new List<Automation>();
            var dataHolder = await _db.Automations.ToListAsync();

            if (dataHolder != null && dataHolder.Count > 0)
            {
				foreach (var item in dataHolder)
				{
					if (item.GuildId == Guild.Id)
					{
						AutomationList.Add(item);
					}
				}
			}
            else
            {
                var tempItem = new Automation()
                {
                    GuildId = guildId,
                    Id = 0
                };

				AutomationList.Add(tempItem);
				var context = DBContextFactory.GetNewContext();
				context.Automations.Add(tempItem);
			}
            Automations = AutomationList;
		}

        public async Task OnGetWithAlert(ulong guildId, string message)
        {
            await OnGet(guildId);
            ViewData["Message"] = message;
        }

        public async Task<IActionResult> OnPostSave(Automation Automation)
        {
            var vc = await _db.Automations.FirstOrDefaultAsync(v => v.GuildId == Automation.GuildId);
            if (vc == null)
            {
                return BadRequest();
            }

            await _db.SaveChangesAsync();
            return RedirectToPage("Index", "WithAlert", new { guildId = vc.GuildId, message = $"Saved changes to channel {vc.GuildId}" });
        }
    }
}
