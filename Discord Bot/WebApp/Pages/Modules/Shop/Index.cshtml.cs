using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;
using WebApp.Pages.Partials;

namespace WebApp.Pages.Modules.Shop
{
    [Authorize(Policy = "Allowed")]

	public class IndexModel : PageModel
    {
        public Guild Guild { get; set; }

        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }

		public List<ShopItem> ShopItems { get; set; }

        public ShopItem SavedItem { get; set; }

		public List<Currency> AllCurrencies { get; set; }

		public ulong guildId { get; set; }

		public async Task OnGet(ulong guildId)
        {
			Guild = await _db.Guilds.FirstOrDefaultAsync(g => g.Id == guildId);
            
            //Get shop items
            var newList = new List<ShopItem>();

            newList = await _db.ShopItems
				.Where(a => a.GuildId == guildId)
				.ToListAsync() ?? new List<ShopItem>();

            //If no items
            if (newList.Count == 0)
            {
                var tempItem = new ShopItem() {
                    GuildId = guildId,
                    ItemName = "No name",
                    Cost = 0,
                    Description = "No description",
                };

                _db.ShopItems.Add(tempItem);
				newList.Add(tempItem);

				await _db.SaveChangesAsync();
			}

			ShopItems = newList;
			AllCurrencies = _db.Currencies.ToList();
            Console.WriteLine(AllCurrencies[0].Name);
		}

        public async Task<IActionResult> OnPostCreateNewItem(ulong guildId)
        {
			var tempItem = new ShopItem()
			{
				GuildId = guildId,
				ItemName = "No name",
				Cost = 0,
				Description = "No description",
			};
			_db.ShopItems.Add(tempItem);

			await _db.SaveChangesAsync();
			return RedirectToPage("Index", "WithAlert", new { guildId = guildId, message = $"Added new ShopItem" });
		}


		public async Task OnGetWithAlert(ulong guildId, string message)
        {
            await OnGet(guildId);
            ViewData["Message"] = message;
        }

		public async Task<IActionResult> OnPostDelete(ShopItem SavedItem)
		{
			var updateItem = await _db.ShopItems.FirstOrDefaultAsync(v => v.Id == SavedItem.Id);

			_db.ShopItems.Remove(updateItem);

			await _db.SaveChangesAsync();

			return RedirectToPage("Index", "WithAlert", new { guildId = SavedItem.GuildId, message = $"Deleted Shop item" });
		}

		public async Task<IActionResult> OnPostSave(ShopItem SavedItem)
        {
			var updateItem = await _db.ShopItems.FirstOrDefaultAsync(v => v.Id == SavedItem.Id);

			if (updateItem == null)
			{
				return BadRequest();
			}

            updateItem.Id = SavedItem.Id;
			updateItem.GuildId = SavedItem.GuildId;
			updateItem.ItemName = SavedItem.ItemName;
			updateItem.emojiId = SavedItem.emojiId;
			updateItem.CurrencyId = SavedItem.CurrencyId;
			updateItem.Cost = SavedItem.Cost;
			updateItem.Description = SavedItem.Description;

            await _db.SaveChangesAsync();

			return RedirectToPage("Index","WithAlert",new { guildId = SavedItem.GuildId, message = $"Saved changes to {SavedItem.GuildId}" });
        }
    }
}
