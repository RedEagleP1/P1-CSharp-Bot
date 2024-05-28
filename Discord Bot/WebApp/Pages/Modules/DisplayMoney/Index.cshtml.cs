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
using System.Text.Json;
using WebApp.Pages.Modules.CurrencyResets;
using WebApp.Pages.Partials;
using System.IO;

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

        private static readonly HttpClient client = new HttpClient();

        public async Task OnGet(ulong guildId)
        {
            Guild = await _db.Guilds.FirstOrDefaultAsync(g => g.Id == guildId);
            CurrencyOwned = await _db.CurrenciesOwned.ToListAsync();
            List<DisplayList> displaylists = new();
            var jsonText = System.IO.File.ReadAllText("appsettings.json");
            var botToken = "";

            client.DefaultRequestHeaders.Clear();

            //Gets information out of the json
            using (JsonDocument doc = JsonDocument.Parse(jsonText))
            {
                JsonElement root = doc.RootElement;
                botToken = root.GetProperty("Discord").GetProperty("botToken").GetString();
            }
            client.DefaultRequestHeaders.Add("Authorization", $"Bot {botToken}");

            foreach (CurrencyOwned currencyOwned in CurrencyOwned)
            {
                var currencyRef = _db.Currencies.FirstOrDefault(g => g.Id == currencyOwned.Id);

                //This is such a weird way of doing things, but the code works.
                if (currencyRef != null)
                {
                    //API request, uses bot token for authorization and sends it to a URL to get member information.
                    string url = "https://discord.com/api/v9/users/"+$"{currencyOwned.OwnerId}\n";
                    var response = await client.GetAsync(url);
                    var responseData = await response.Content.ReadAsStringAsync();
                    string username = "";

                    //Gets information out of the json
                    using (JsonDocument doc = JsonDocument.Parse(responseData))
                    {
                        JsonElement root = doc.RootElement;
                        username = root.GetProperty("username").GetString();
                    }

                    //Adds to the displaylist including the username of the user from the JSON.
                    DisplayList displayList = new DisplayList()
                    {
                        CurrencyName = currencyRef.Name,
                        UserName = username,
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
