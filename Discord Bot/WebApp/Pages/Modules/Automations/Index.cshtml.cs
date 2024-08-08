using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Diagnostics;
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
		public AutomationPackage SavedInfo { get; set; }

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

				List<IdAuto> tempWhenList = new List<IdAuto>();
				List<IdAuto> tempIfList = new List<IdAuto>();
				List<IdAuto> tempDoList = new List<IdAuto>();

				foreach (var option in optionsForItem)
				{
					var tempId = new IdAuto
					{
						AutomationId = option.AutomationId,
						Id = option.Id,
						SelectedOption = option.SelectedOption,
						Type = option.Type,
						Value = option.Value,
					};

					if (tempId.Type == 0)
					{
						tempWhenList.Add(tempId);
					}
					else if (tempId.Type == 1)
					{
						tempIfList.Add(tempId);
					}
					else if (tempId.Type == 2)
					{
						tempDoList.Add(tempId);
					}

					tempItem.When = tempWhenList;
					tempItem.If = tempIfList;
					tempItem.Do = tempDoList;
				}

				AutomationList.Add(tempItem);
			}

			if (!dataHolder.Any())
			{
				var tempItem = new Automation
				{
					GuildId = guildId
				};

				_db.Automations.Add(tempItem);
				await _db.SaveChangesAsync();

				var tempWhen = new IdAuto
				{
					SelectedOption = -1,
					Type = 0,
					Value = "",
					AutomationId = tempItem.Id
				};
				var tempIf = new IdAuto
				{
					SelectedOption = -1,
					Type = 1,
					Value = "",
					AutomationId = tempItem.Id
				};
				var tempDo = new IdAuto
				{
					SelectedOption = -1,
					Type = 2,
					Value = "",
					AutomationId = tempItem.Id
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

				_db.IdAutos.Add(tempWhen);
				_db.IdAutos.Add(tempIf);
				_db.IdAutos.Add(tempDo);
				await _db.SaveChangesAsync();
			}

			Packages = AutomationList;
		}


		public async Task OnGetWithAlert(ulong guildId, string message)
        {
            await OnGet(guildId);
            ViewData["Message"] = message;
        }

        public async Task<IActionResult> OnPostSave(AutomationPackage SavedInfo)
        {
			var updateAutomation = await _db.Automations.FirstOrDefaultAsync(v => v.Id == SavedInfo.Auto.Id && v.GuildId == SavedInfo.Auto.GuildId);

			Console.WriteLine("1!");

			Console.WriteLine(SavedInfo.Auto.Id);

			if (updateAutomation == null)
			{
				return BadRequest();
			}

			Console.WriteLine("2!");

			//Update Autos

			var combinedLists = new List<IdAuto>();
			combinedLists.AddRange(SavedInfo.When);
			combinedLists.AddRange(SavedInfo.If);
			combinedLists.AddRange(SavedInfo.Do);

			Console.WriteLine("3!");

			foreach (var auto in combinedLists)
            {
				var selectedAuto = await _db.IdAutos.FirstOrDefaultAsync(v => v.Id == auto.Id);
                if (selectedAuto != null)
                {
                    selectedAuto = auto;
                }
			}

			Console.WriteLine("4!");

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
