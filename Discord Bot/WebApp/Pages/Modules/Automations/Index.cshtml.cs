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
		public List<AutomationInfo> AfterAutomations { get; set; }
		public List<AutomationInfo> AutomationInfos { get; set; }
        public List<AutomationPackage> Packages { get; set; }
		public AutomationPackage SavedInfo { get; set; }

		public ulong guildId { get; set; }

		public Automation ReferenceAuto { get; set; }
		public int chosenType { get; set; }

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
				List<IdAuto> tempAfterList = new List<IdAuto>();

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
					else if (tempId.Type == 3)
					{
						tempAfterList.Add(tempId);
					}

					tempItem.When = tempWhenList;
					tempItem.If = tempIfList;
					tempItem.Do = tempDoList;
					tempItem.After = tempAfterList;
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
					SelectedOption = 0,
					Type = 0,
					Value = "",
					AutomationId = tempItem.Id
				};
				var tempIf = new IdAuto
				{
					SelectedOption = 0,
					Type = 1,
					Value = "",
					AutomationId = tempItem.Id
				};
				var tempDo = new IdAuto
				{
					SelectedOption = 0,
					Type = 2,
					Value = "",
					AutomationId = tempItem.Id
				};
				var tempAfter = new IdAuto
				{
					SelectedOption = 0,
					Type = 3,
					Value = "",
					AutomationId = tempItem.Id
				};

				List<IdAuto> tempWhenList = new List<IdAuto>();
				List<IdAuto> tempIfList = new List<IdAuto>();
				List<IdAuto> tempDoList = new List<IdAuto>();
				List<IdAuto> tempAfterList = new List<IdAuto>();

				tempWhenList.Add(tempWhen);
				tempIfList.Add(tempIf);
				tempDoList.Add(tempDo);
				tempAfterList.Add(tempAfter);

				var tempPackage = new AutomationPackage
				{
					Auto = tempItem,
					When = new List<IdAuto>(tempWhenList),
					If = new List<IdAuto>(tempIfList),
					Do = new List<IdAuto>(tempDoList),
					After = new List<IdAuto>(tempAfterList),
				};

				AutomationList.Add(tempPackage);

				_db.IdAutos.Add(tempWhen);
				_db.IdAutos.Add(tempIf);
				_db.IdAutos.Add(tempDo);
				_db.IdAutos.Add(tempAfter);
				await _db.SaveChangesAsync();
			}

			Packages = AutomationList;
		}

		public async Task OnGetWithAlert(ulong guildId, string message)
        {
            await OnGet(guildId);
            ViewData["Message"] = message;
        }

		public async Task<IActionResult> OnPostCreateNewAuto(ulong guildId)
		{
			var tempItem = new Automation
			{
				GuildId = guildId
			};

			_db.Automations.Add(tempItem);
			await _db.SaveChangesAsync();

			var tempWhen = new IdAuto
			{
				SelectedOption = 0,
				Type = 0,
				Value = "",
				AutomationId = tempItem.Id
			};
			var tempIf = new IdAuto
			{
				SelectedOption = 0,
				Type = 1,
				Value = "",
				AutomationId = tempItem.Id
			};
			var tempDo = new IdAuto
			{
				SelectedOption = 0,
				Type = 2,
				Value = "",
				AutomationId = tempItem.Id
			};
			var tempAfter = new IdAuto
			{
				SelectedOption = 0,
				Type = 3,
				Value = "",
				AutomationId = tempItem.Id
			};

			List<IdAuto> tempWhenList = new List<IdAuto>();
			List<IdAuto> tempIfList = new List<IdAuto>();
			List<IdAuto> tempDoList = new List<IdAuto>();
			List<IdAuto> tempAfterList = new List<IdAuto>();

			tempWhenList.Add(tempWhen);
			tempIfList.Add(tempIf);
			tempDoList.Add(tempDo);
			tempAfterList.Add(tempAfter);

			var tempPackage = new AutomationPackage
			{
				Auto = tempItem,
				When = new List<IdAuto>(tempWhenList),
				If = new List<IdAuto>(tempIfList),
				Do = new List<IdAuto>(tempDoList),
				After = new List<IdAuto>(tempAfterList),
			};

			_db.IdAutos.Add(tempWhen);
			_db.IdAutos.Add(tempIf);
			_db.IdAutos.Add(tempDo);
			_db.IdAutos.Add(tempAfter);

			await _db.SaveChangesAsync();
			return RedirectToPage("Index", "WithAlert", new { guildId = guildId, message = $"Added new Automation" });
		}

		public async Task<IActionResult> OnPostCreateNewIf(Automation ReferenceAuto, ulong guildId)
		{
			var tempIdAuto = new IdAuto
			{
				SelectedOption = 0,
				Type = 1,
				Value = "",
				AutomationId = ReferenceAuto.Id
			};

			_db.IdAutos.Add(tempIdAuto);

			await _db.SaveChangesAsync();
			return RedirectToPage("Index", "WithAlert", new { guildId = guildId, message = $"Added new option" });
		}

		public async Task<IActionResult> OnPostCreateNewDo(Automation ReferenceAuto, ulong guildId)
		{
			var tempIdAuto = new IdAuto
			{
				SelectedOption = 0,
				Type = 2,
				Value = "",
				AutomationId = ReferenceAuto.Id
			};

			_db.IdAutos.Add(tempIdAuto);

			await _db.SaveChangesAsync();
			return RedirectToPage("Index", "WithAlert", new { guildId = guildId, message = $"Added new option" });
		}

		public async Task<IActionResult> OnPostDeleteLastIf(Automation ReferenceAuto, ulong guildId)
		{
			var tempIdAuto = await _db.IdAutos.Where(g => g.AutomationId == ReferenceAuto.Id).OrderBy(g => g.Id).LastOrDefaultAsync();
			_db.IdAutos.Remove(tempIdAuto);

			await _db.SaveChangesAsync();
			return RedirectToPage("Index", "WithAlert", new { guildId = guildId, message = $"Removed last added option" });
		}

		public async Task<IActionResult> OnPostDeleteLastDo(Automation ReferenceAuto, ulong guildId)
		{
			var tempIdAuto = await _db.IdAutos.Where(g => g.AutomationId == ReferenceAuto.Id).OrderBy(g => g.Id).LastOrDefaultAsync();
			_db.IdAutos.Remove(tempIdAuto);

			await _db.SaveChangesAsync();
			return RedirectToPage("Index", "WithAlert", new { guildId = guildId, message = $"Removed last added option" });
		}

		public async Task<IActionResult> OnPostDeleteAuto(Automation ReferenceAuto, ulong guildId)
		{
			var AutoIds = await _db.IdAutos.Where(_db => _db.AutomationId == ReferenceAuto.Id).ToListAsync();
			foreach (var AutoId in AutoIds)
			{
				_db.IdAutos.Remove(AutoId);
			}
			_db.Automations.Remove(ReferenceAuto);

			await _db.SaveChangesAsync();
			return RedirectToPage("Index", "WithAlert", new { guildId = guildId, message = $"Removed Automation" });
		}

		public async Task<IActionResult> OnPostSave(AutomationPackage SavedInfo)
        {
			var updateAutomation = await _db.Automations.FirstOrDefaultAsync(v => v.Id == SavedInfo.Auto.Id && v.GuildId == SavedInfo.Auto.GuildId);

			if (updateAutomation == null)
			{
				return BadRequest();
			}

			//Update Autos

			var combinedLists = new List<IdAuto>();
			combinedLists.AddRange(SavedInfo.When);
			combinedLists.AddRange(SavedInfo.If);
			combinedLists.AddRange(SavedInfo.Do);
			combinedLists.AddRange(SavedInfo.After);

			foreach (var auto in combinedLists)
			{
				var selectedAuto = await _db.IdAutos.FirstOrDefaultAsync(v => v.Id == auto.Id);
				if (selectedAuto != null)
				{
					selectedAuto.Id = auto.Id;
					selectedAuto.SelectedOption = auto.SelectedOption;
					selectedAuto.Value = auto.Value;
					selectedAuto.Type = auto.Type;
				}
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
		public List<IdAuto>? After { get; set; }
	}
}
