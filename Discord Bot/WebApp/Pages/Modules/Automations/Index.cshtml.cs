using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;
using WebApp.Pages.Partials;

namespace WebApp.Pages.Modules.Automations
{
    [Authorize(Policy = "Allowed")]
    public class IndexModel : PageModel
    {
        public Guild Guild { get; set; }
        public List<Automation> WhenAutomations { get; set; }
        public List<Automation> DoAutomations { get; set; }
        public List<Automation> IfAutomations { get; set; }
        public VoiceChannelCurrencyGain VoiceChannelCurrencyGain { get; set; }

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
        }

        public async Task OnGetWithAlert(ulong guildId, string message)
        {
            await OnGet(guildId);
            ViewData["Message"] = message;
        }

        /*public async Task<IActionResult> OnPostSave(VoiceChannelCurrencyGain VoiceChannelCurrencyGain)
        {
            var vc = await _db.VoiceChannelCurrencyGains.FirstOrDefaultAsync(v => v.Id == VoiceChannelCurrencyGain.Id);
            if(vc == null)
            {
                return BadRequest();
            }

            vc.AmountGainedPerHourWhenMuteOrDeaf = VoiceChannelCurrencyGain.AmountGainedPerHourWhenMuteOrDeaf;
            vc.AmountGainedPerHourWhenSpeaking = VoiceChannelCurrencyGain.AmountGainedPerHourWhenSpeaking;
            vc.CurrencyId = VoiceChannelCurrencyGain.CurrencyId;
            vc.IsEnabled = VoiceChannelCurrencyGain.IsEnabled;

            await _db.SaveChangesAsync();
            return RedirectToPage("Index", "WithAlert", new { guildId = vc.GuildId, message = $"Saved changes to channel {vc.ChannelName}" });
        }*/
    }
}
