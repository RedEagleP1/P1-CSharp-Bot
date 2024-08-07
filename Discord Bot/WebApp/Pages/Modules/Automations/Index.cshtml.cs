using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Reactive.Linq;
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
		public List<AutomationInfo> AutomationInfos { get; set; }
        public List<AutomationPackage> Packages { get; set; }
		public AutomationPackage AutomationPackage { get; set; }

		private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }
		public async Task OnGet(ulong guildId)
		{

			Guild = await _db.Guilds.FirstOrDefaultAsync(g => g.Id == guildId);
			var dropModel = new AutomationDropdownModel();

			// Add Lists
			WhenAutomations = dropModel.con_When.ToList();
			DoAutomations = dropModel.con_Do.ToList();
			IfAutomations = dropModel.con_If.ToList();

			// Get Info
			var dataHolder = await _db.Automations
				.Where(a => a.GuildId == guildId)
				.ToListAsync() ?? new List<Automation>();

			var automationIds = dataHolder.Select(a => a.Id).ToList();

			var optionHolder = await _db.IdAutos
				.Where(i => automationIds.Contains(i.AutomationId))
				.ToListAsync();

			var AutomationList = new List<AutomationPackage>();

			foreach (var item in dataHolder)
			{
				var tempItem = new AutomationPackage { Auto = item };

				var optionsForItem = optionHolder.Where(o => o.AutomationId == item.Id);

				foreach (var option in optionsForItem)
				{
					switch (option.SelectedOption)
					{
						case 0:
							tempItem.When.Add(option);
							break;
						case 1:
							tempItem.If.Add(option);
							break;
						case 2:
							tempItem.Do.Add(option);
							break;
						default:
							break;
					}
				}

				//AutomationList.Add(tempItem);
			}

			if (dataHolder.Any())
			{
				var tempItem = new Automation
				{
					GuildId = guildId,
					Id = 0
				};
				var tempWhen = new IdAuto
				{
					Id = 0,
					SelectedOption = -1,
					Type = 0,
					Value = "",
					AutomationId = 0
				};
				var tempIf = new IdAuto
				{
					Id = 0,
					SelectedOption = -1,
					Type = 1,
					Value = "",
					AutomationId = 0
				};
				var tempDo = new IdAuto
				{
					Id = 0,
					SelectedOption = -1,
					Type = 2,
					Value = "",
					AutomationId = 0
				};

				List<IdAuto> tempWhenList = new List<IdAuto>();
				List<IdAuto> tempIfList = new List<IdAuto>();
				List<IdAuto> tempDoList = new List<IdAuto>();

				tempWhenList.Add(tempWhen);
				tempIfList.Add(tempIf);
				tempDoList.Add(tempDo);

				var tempPackage = new AutomationPackage
				{
					Auto = tempItem,
					When = new List<IdAuto>(tempWhenList),
					If = new List<IdAuto>(tempIfList),
					Do = new List<IdAuto>(tempDoList),
				};

				AutomationList.Add(tempPackage);

				//_db.Automations.Add(tempItem);
				//await _db.SaveChangesAsync();

				Console.WriteLine(AutomationList.Count);
			}

			Packages = AutomationList;

			//Console.WriteLine(Packages[0].When[0].Id);
		}


		public async Task OnGetWithAlert(ulong guildId, string message)
        {
            await OnGet(guildId);
            ViewData["Message"] = message;
        }

        public async Task<IActionResult> OnPostSave(AutomationPackage SavedInfo)
        {
			var updateAutomation = await _db.Automations.FirstOrDefaultAsync(v => v.Id == SavedInfo.Auto.Id && v.GuildId == SavedInfo.Auto.GuildId);

			if (updateAutomation == null)
			{
				return BadRequest();
			}

			//When
			var i = 0;

            foreach (var auto in SavedInfo.When)
            {
				var selectedAuto = await _db.IdAutos.FirstOrDefaultAsync(v => v.AutomationId == SavedInfo.Auto.Id && v.Type == 0 && v.Id == i);
                if (selectedAuto != null)
                {
                    selectedAuto = auto;
                }
                i++;
			}
			//If
			i = 0;

			foreach (var auto in SavedInfo.If)
			{
				var selectedAuto = await _db.IdAutos.FirstOrDefaultAsync(v => v.AutomationId == SavedInfo.Auto.Id && v.Type == 1 && v.Id == i);
				if (selectedAuto != null)
				{
					selectedAuto = auto;
				}
				i++;
			}
			//Do
			i = 0;

			foreach (var auto in SavedInfo.Do)
			{
				var selectedAuto = await _db.IdAutos.FirstOrDefaultAsync(v => v.AutomationId == SavedInfo.Auto.Id && v.Type == 2 && v.Id == i);
				if (selectedAuto != null)
				{
					selectedAuto = auto;
				}
				i++;
			}

			await _db.SaveChangesAsync();
			return RedirectToPage("Index", "WithAlert", new { guildId = SavedInfo.Auto.GuildId, message = $"Saved changes to {SavedInfo.Auto.GuildId}" });
		}
	}

    public class AutomationPackage
    {
        public Automation? Auto { get; set; }
        public List<IdAuto>? When { get; set; }
        public List<IdAuto>? If { get; set; }
		public List<IdAuto>? Do { get; set; }
	}
}
