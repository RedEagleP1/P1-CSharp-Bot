using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Diagnostics;
using WebApp.Pages.Partials;

namespace WebApp.Pages.Modules.CurrencyResets
{
    [Authorize(Policy = "Allowed")]
    public class IndexModel : PageModel
    {
        public Guild Guild { get; set; }
        public List<Currency> AllCurrencies { get; set; }
        public List<CurrencyResetModel> CurrencyResets { get; set; }
        public CurrencyReset CurrencyReset { get; set; }

        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task OnGet(ulong guildId)
        {
            Guild = await _db.Guilds.FirstOrDefaultAsync(g => g.Id == guildId);
            AllCurrencies = _db.Currencies.ToList();
            List<CurrencyResetModel> resetModels = new();
            var context = DBContextFactory.GetNewContext();

            foreach (var currency in AllCurrencies)
            {
                var newCurrencyReset = await _db.CurrencyResets.FirstOrDefaultAsync(v => v.CurrencyId == currency.Id);

                if (newCurrencyReset == null) 
                {
                    newCurrencyReset = new CurrencyReset()
                    {
                        Id = -1,
                        GuildId = guildId,
                        DaysBetween = 30,
                        Auto = false,
                        CurrencyId = currency.Id,
                    };
                    //Console.WriteLine(context.ContextId);
                    //context.CurrencyResets.Add(newCurrencyReset);
                }

                var newResetModel = new CurrencyResetModel()
                {
                    CurrencyReset = newCurrencyReset,
                    CurrencyName = currency.Name,
                };

                resetModels.Add(newResetModel);
            }

            CurrencyResets = resetModels;
        }

        public async Task OnGetWithAlert(ulong guildId, string message)
        {
            await OnGet(guildId);
            ViewData["Message"] = message;
        }

        public async Task<IActionResult> OnPostSave(CurrencyReset CurrencyReset)
        {
            var Info = await _db.CurrencyResets.FirstOrDefaultAsync(v => v.CurrencyId == CurrencyReset.CurrencyId);
            if (Info == null)
            {
                Console.WriteLine("oops!");
                return BadRequest();
            }

            Info.Id = CurrencyReset.Id;
            Info.GuildId = CurrencyReset.GuildId;
            Info.DaysBetween = CurrencyReset.DaysBetween;
            Info.Auto = CurrencyReset.Auto;
            Info.CurrencyId = CurrencyReset.CurrencyId;

            await _db.SaveChangesAsync();

            return RedirectToPage("Index", "WithAlert", new { guildId = Info.GuildId, message = $"Saved changes to {Info.GuildId}" });
        }
    }

    public class CurrencyResetModel
    {
        public CurrencyReset CurrencyReset { get; set; }
        public string CurrencyName { get; set; }
    }
}
