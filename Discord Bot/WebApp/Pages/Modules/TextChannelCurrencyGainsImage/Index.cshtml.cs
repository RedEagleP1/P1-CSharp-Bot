using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;

namespace WebApp.Pages.Modules.TextChannelCurrencyGainsImage
{
    [Authorize(Policy = "Allowed")]
    public class IndexModel : PageModel
    {
        public Guild Guild { get; set; }
        public List<TextChannelCurrencyGainImageModel> TextChannelCurrencyGainsImages { get; set; }
        public List<Currency> AllCurrencies { get; set; }
        public TextChannelCurrencyGainImage TextChannelCurrencyGainImage { get; set; }

        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task OnGet(ulong guildId)
        {
            Guild = await _db.Guilds.FirstOrDefaultAsync(g => g.Id == guildId);
            List<TextChannelCurrencyGainImageModel> currencyGainModels = new();
            foreach (var textChannel in _db.TextChannelsCurrencyGainImage.Where(v => v.GuildId == guildId).ToList())
            {
                var currency = await _db.Currencies.FirstOrDefaultAsync(c => c.Id == textChannel.CurrencyId);
                currencyGainModels.Add(new TextChannelCurrencyGainImageModel()
                {
                    TextChannelCurrencyGainImage = textChannel,
                    CurrencyName = currency?.Name ?? "None"
                });
            }

            TextChannelCurrencyGainsImages = currencyGainModels;
            AllCurrencies = _db.Currencies.ToList();
        }

        public async Task OnGetWithAlert(ulong guildId, string message)
        {
            await OnGet(guildId);
            ViewData["Message"] = message;
        }

        public async Task<IActionResult> OnPostSave(TextChannelCurrencyGainImage TextChannelCurrencyGainImage)
        {
            var textChannel = await _db.TextChannelsCurrencyGainImage.FirstOrDefaultAsync(v => v.Id == TextChannelCurrencyGainImage.Id);
            if (textChannel == null)
            {
                return BadRequest();
            }

            textChannel.AmountGainedPerImagePost = TextChannelCurrencyGainImage.AmountGainedPerImagePost;
            textChannel.DelayBetweenAllowedImagePostInMinutes = TextChannelCurrencyGainImage.DelayBetweenAllowedImagePostInMinutes;
            textChannel.CurrencyId = TextChannelCurrencyGainImage.CurrencyId;
            textChannel.IsEnabled = TextChannelCurrencyGainImage.IsEnabled;

            await _db.SaveChangesAsync();
            return RedirectToPage("Index", "WithAlert", new { guildId = textChannel.GuildId, message = $"Saved changes to channel {textChannel.ChannelName}" });
        }
    }
    public class TextChannelCurrencyGainImageModel
    {
        public TextChannelCurrencyGainImage TextChannelCurrencyGainImage { get; set; }
        public string CurrencyName { get; set; }
    }
}

