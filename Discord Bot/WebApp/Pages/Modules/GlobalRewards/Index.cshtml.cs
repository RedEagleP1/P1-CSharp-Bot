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
        public GlobalVoiceCurrencyGain GlobalCurrencyGain { get; set; }

        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task OnGet(ulong guildId)
        {
            Guild = await _db.Guilds.FirstOrDefaultAsync(g => g.Id == guildId);
            Console.WriteLine(Guild.Name);
            AllCurrencies = _db.Currencies.ToList();
        }

        public async Task OnGetWithAlert(ulong guildId, string message)
        {
            await OnGet(guildId);
            ViewData["Message"] = message;
        }

        public async Task<IActionResult> OnPostSave(GlobalVoiceCurrencyGain GlobalCurrencyGain)
        {
            var globalInfo = await _db.GlobalVoiceCurrencyGains.FirstOrDefaultAsync(v => v.GuildId == GlobalCurrencyGain.GuildId);
            if (globalInfo == null)
            {
                return BadRequest();
            }

            return RedirectToPage("Index","WithAlert",new { guildId = Guild.Id, message = $"Saved changes to {Guild.Name}" });
        }
    }
}
