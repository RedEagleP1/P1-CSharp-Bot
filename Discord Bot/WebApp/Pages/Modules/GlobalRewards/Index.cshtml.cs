using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;
using WebApp.Pages.Partials;

namespace WebApp.Pages.Modules.GlobalRewards
{
    [Authorize(Policy = "Allowed")]
    public class IndexModel : PageModel
    {
        public Guild Guild { get; set; }
        public List<Currency> AllCurrencies { get; set; }
        public GlobalVoiceCurrencyGain GlobalVoiceCurrencyGain { get; set; }

        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task OnGet(ulong guildId)
        {
            Guild = await _db.Guilds.FirstOrDefaultAsync(g => g.Id == guildId);
            GlobalVoiceCurrencyGain = await _db.GlobalVoiceCurrencyGains.FirstOrDefaultAsync(g => g.GuildId == guildId);
            if (GlobalVoiceCurrencyGain == null)
            {
                var newItem = new GlobalVoiceCurrencyGain()
                {
                    GuildId = guildId,
                    IsEnabled = false
                };
                _db.GlobalVoiceCurrencyGains.Add(newItem);

				GlobalVoiceCurrencyGain = await _db.GlobalVoiceCurrencyGains.FirstOrDefaultAsync(g => g.GuildId == guildId);
			}
			await _db.SaveChangesAsync();
			AllCurrencies = _db.Currencies.ToList();
        }

        public async Task OnGetWithAlert(ulong guildId, string message)
        {
            await OnGet(guildId);
            ViewData["Message"] = message;
        }

        public async Task<IActionResult> OnPostSave(GlobalVoiceCurrencyGain GlobalVoiceCurrencyGain)
        {
            var globalInfo = await _db.GlobalVoiceCurrencyGains.FirstOrDefaultAsync(v => v.GuildId == GlobalVoiceCurrencyGain.GuildId);
            if (globalInfo == null)
            {
                return BadRequest();
            }

            globalInfo.AmountGainedPerHourWhenMuteOrDeaf = GlobalVoiceCurrencyGain.AmountGainedPerHourWhenMuteOrDeaf;
            globalInfo.AmountGainedPerHourWhenSpeaking = GlobalVoiceCurrencyGain.AmountGainedPerHourWhenSpeaking;
            globalInfo.CurrencyId = GlobalVoiceCurrencyGain.CurrencyId;
            globalInfo.IsEnabled = GlobalVoiceCurrencyGain.IsEnabled;

            await _db.SaveChangesAsync();

            return RedirectToPage("Index","WithAlert",new { guildId = globalInfo.GuildId, message = $"Saved changes to {globalInfo.GuildId}" });
        }
    }
}
