using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Diagnostics;
using WebApp.Pages.Modules.CurrencyResets;
using WebApp.Pages.Partials;

namespace WebApp.Pages.Modules.DisplayMoney
{
    [Authorize(Policy = "Allowed")]
    public class IndexModel : PageModel
    {
        public Guild Guild { get; set; }
        public List<CurrencyOwned> CurrencyOwned { get; set; }

        public List<DisplayList> DisplayLists { get; set; }

        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task OnGet(ulong guildId)
        {
            var client = DiscordREST.discordRestClient;
            Guild = await _db.Guilds.FirstOrDefaultAsync(g => g.Id == guildId);
            CurrencyOwned = await _db.CurrenciesOwned.ToListAsync();
            List<DisplayList> displaylists = new();
            var context = DBContextFactory.GetNewContext();

            foreach (CurrencyOwned currencyOwned in CurrencyOwned)
            {
                var currencyRef = _db.Currencies.FirstOrDefault(g => g.Id == currencyOwned.Id);

                Debug.WriteLine($"Name:{currencyRef.Name} UserName:{currencyOwned.OwnerId} Amount:{currencyOwned.Amount}");

                if (currencyRef != null)
                {
                    var user = await client.GetUserAsync(currencyOwned.OwnerId);
                    DisplayList displayList = new DisplayList()
                    {
                        CurrencyName = currencyRef.Name,
                        UserName = user.Username,
                        amount = currencyOwned.Amount,
                    };
                    displaylists.Add(displayList);
                }
            }

            DisplayLists = displaylists;
        }


        public async Task OnGetWithAlert(ulong guildId, string message)
        {
            await OnGet(guildId);
            ViewData["Message"] = message;
        }

        public class DisplayList
        {
            public string CurrencyName = "";
            public string UserName = "";
            public float amount = 0.0f;
        }
    }
}
