using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
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
            AllCurrencies = await _db.Currencies.ToListAsync();
            List<CurrencyResetModel> resetModels = new();

            foreach (var currency in AllCurrencies)
            {
                var newCurrencyReset = await _db.CurrencyResets.FirstOrDefaultAsync(v => v.CurrencyId == currency.Id);

                if (newCurrencyReset == null)
                {
                    newCurrencyReset = new CurrencyReset()
                    {
                        GuildId = guildId,
                        DaysBetween = 30,
                        Auto = false,
                        CurrencyId = currency.Id,
                        DaysLeft = 30,
                    };

                    _db.CurrencyResets.Add(newCurrencyReset);
                    await _db.SaveChangesAsync();
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

        public async Task<IActionResult> OnPostSave(CurrencyReset CurrencyReset, string action)
        {
            var Info = await _db.CurrencyResets.FirstOrDefaultAsync(v => v.CurrencyId == CurrencyReset.CurrencyId);
            if (Info == null)
            {
                return BadRequest();
            }

            if (action == "save")
            {

                Info.GuildId = CurrencyReset.GuildId;
                Info.DaysBetween = CurrencyReset.DaysBetween;
                Info.Auto = CurrencyReset.Auto;
                Info.CurrencyId = CurrencyReset.CurrencyId;
                if (Info.Auto)
                {
                    var today = DateTime.UtcNow.AddHours(-6); // CST conversion
                    var firstDayOfNextMonth = new DateTime(today.Year, today.Month, 1).AddMonths(1);
                    Info.DaysLeft = (firstDayOfNextMonth - today).Days;
                }
                else
                {
                    Info.DaysLeft = CurrencyReset.DaysBetween;
                }

                await _db.SaveChangesAsync();

                return RedirectToPage("Index", "WithAlert", new { guildId = Info.GuildId, message = $"Saved changes to {Info.GuildId}" });
            }
            else
            {
                var CurInfo = await _db.Currencies.FirstOrDefaultAsync(v => v.Id == Info.CurrencyId);

                foreach (var currency in _db.CurrenciesOwned)
                {
                    if (currency != null)
                    {
                        if (currency.CurrencyId == Info.CurrencyId)
                        {
                            currency.Amount = 0;
                        }
                    }
                }

                await _db.SaveChangesAsync();

                return RedirectToPage("Index", "WithAlert", new { guildId = Info.GuildId, message = $"Set {CurInfo.Name} to 0 for ALL users." });
            }
        }
    }

    public class CurrencyResetModel
    {
        public CurrencyReset CurrencyReset { get; set; }
        public string CurrencyName { get; set; }
    }
}
