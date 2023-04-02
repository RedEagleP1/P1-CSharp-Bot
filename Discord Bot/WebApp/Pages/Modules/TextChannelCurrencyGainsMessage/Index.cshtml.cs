using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;

namespace WebApp.Pages.Modules.TextChannelCurrencyGainsMessage
{
    [Authorize(Policy = "Allowed")]
    public class IndexModel : PageModel
    {
        public Guild Guild { get; set; }
        public List<TextChannelCurrencyGainMessageModel> TextChannelCurrencyGainsMessage { get; set; }
        public List<Currency> AllCurrencies { get; set; }
        public TextChannelCurrencyGainMessage TextChannelCurrencyGainMessage { get; set; }

        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task OnGet(ulong guildId)
        {
            Guild = await _db.Guilds.FirstOrDefaultAsync(g => g.Id == guildId);
            List<TextChannelCurrencyGainMessageModel> currencyGainModels = new();
            foreach (var textChannel in _db.TextChannelsCurrencyGainMessage.Where(v => v.GuildId == guildId).ToList())
            {
                var currency = await _db.Currencies.FirstOrDefaultAsync(c => c.Id == textChannel.CurrencyId);
                currencyGainModels.Add(new TextChannelCurrencyGainMessageModel()
                {
                    TextChannelCurrencyGainMessage = textChannel,
                    CurrencyName = currency?.Name ?? "None"
                });
            }

            TextChannelCurrencyGainsMessage = currencyGainModels;
            AllCurrencies = _db.Currencies.ToList();
        }

        public async Task OnGetWithAlert(ulong guildId, string message)
        {
            await OnGet(guildId);
            ViewData["Message"] = message;
        }

        public async Task<IActionResult> OnPostSave(TextChannelCurrencyGainMessage TextChannelCurrencyGainMessage)
        {
            var textChannel = await _db.TextChannelsCurrencyGainMessage.FirstOrDefaultAsync(v => v.Id == TextChannelCurrencyGainMessage.Id);
            if (textChannel == null)
            {
                return BadRequest();
            }

            textChannel.AmountGainedPerMessage = TextChannelCurrencyGainMessage.AmountGainedPerMessage;
            textChannel.DelayBetweenAllowedMessageInMinutes = TextChannelCurrencyGainMessage.DelayBetweenAllowedMessageInMinutes;
            textChannel.CurrencyId = TextChannelCurrencyGainMessage.CurrencyId;
            textChannel.IsEnabled = TextChannelCurrencyGainMessage.IsEnabled;

            await _db.SaveChangesAsync();
            return RedirectToPage("Index", "WithAlert", new { guildId = textChannel.GuildId, message = $"Saved changes to channel {textChannel.ChannelName}" });
        }
    }
    public class TextChannelCurrencyGainMessageModel
    {
        public TextChannelCurrencyGainMessage TextChannelCurrencyGainMessage { get; set; }
        public string CurrencyName { get; set; }
    }
}
