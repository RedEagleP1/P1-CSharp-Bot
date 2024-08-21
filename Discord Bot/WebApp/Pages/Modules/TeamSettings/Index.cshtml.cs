using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models;
using System.Diagnostics;
using System.Reactive.Linq;
using WebApp.Pages.Partials;

namespace WebApp.Pages.Modules.TeamSettings
{
	[Authorize(Policy = "Allowed")]
	public class IndexModel : PageModel
	{ 
		public Guild Guild { get; set; }

		public Models.TeamSettings SavedInfo { get; set; }

		public Models.TeamSettings TeamSettingsValues { get; set; } = null;


		private readonly ApplicationDbContext _db;
		public IndexModel(ApplicationDbContext db)
		{
			_db = db;
		}
		public async Task OnGet(ulong guildId)
		{
			Guild = await _db.Guilds.FirstOrDefaultAsync(g => g.Id == guildId);
			if (Guild == null)
				Console.WriteLine($"ERROR: TeamSettings module could not find guild with Id {guildId}.");

			TeamSettingsValues = await _db.TeamSettings.FirstOrDefaultAsync(_db => _db.GuildId == guildId);
			if (TeamSettingsValues == null)
			{
				// There is no TeamSettings record for this guild yet, so create one.
				TeamSettingsValues = Models.TeamSettings.CreateDefault(guildId);

				// Save the new record to the database.
				_db.TeamSettings.Add(TeamSettingsValues);
				await _db.SaveChangesAsync();

				// Reload the new record to get its Id.
				TeamSettingsValues = await _db.TeamSettings.FirstOrDefaultAsync(_db => _db.GuildId == guildId);
			}
		}

		public async Task OnGetWithAlert(ulong guildId, string message)
		{
			await OnGet(guildId);
			ViewData["Message"] = message;
		}

		public async Task<IActionResult> OnPostSave(Models.TeamSettings teamSettingsValues)
		{
			string errorMsg = string.Empty;

			if (teamSettingsValues.MaxOrgsPerLegion < 1)
				errorMsg = "MaxOrgsPerLegion must be a non-zero positive number!";
			if (teamSettingsValues.MaxMembersPerOrg < 1)
				errorMsg = "MaxMembersPerOrg must be a non-zero positive number!";

			if (!string.IsNullOrWhiteSpace(errorMsg))
			{
				ViewData["Message"] = $"Failed to save changes to {teamSettingsValues.GuildId}: {errorMsg}";
				return RedirectToPage("Index", "WithAlert", new { guildId = teamSettingsValues.GuildId, message = $"Failed to saved changes to {teamSettingsValues.GuildId}: {errorMsg}" });
			}


			_db.TeamSettings.Update(teamSettingsValues);
			await _db.SaveChangesAsync();

			return RedirectToPage("Index", "WithAlert", new { guildId = teamSettingsValues.GuildId, message = $"Saved changes to {teamSettingsValues.GuildId}." });
		}
	}
}
