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
        public VoiceChannelCurrencyGain VoiceChannelCurrencyGain { get; set; }

        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task OnGet(ulong guildId)
        {
            Guild = await _db.Guilds.FirstOrDefaultAsync(g => g.Id == guildId);
            AllCurrencies = _db.Currencies.ToList();
        }

        public async Task OnGetWithAlert(ulong guildId, string message)
        {
            await OnGet(guildId);
            ViewData["Message"] = message;
        }

        public async Task<IActionResult> OnPostSave(VoiceChannelCurrencyGain VoiceChannelCurrencyGain)
        {
            return RedirectToPage("Index","WithAlert",new { guildId = Guild.Id, message = $"Saved changes to channel {Guild.Name}" });
        }
    }

    public class VoiceChannelCurrencyGainModel
    {
        public VoiceChannelCurrencyGain VoiceChannelCurrencyGain { get; set; }
        public string CurrencyName { get; set; }
    }
}
