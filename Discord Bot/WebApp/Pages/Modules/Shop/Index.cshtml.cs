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

        public List<ShopItem> SavedItems { get; set; }

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
                    ItemName = "",
                    Cost = 0,
                    ItemEffectVal = "",
                };

                _db.ShopItems.Add(tempItem);
                ShopItems.Add(tempItem);

				await _db.SaveChangesAsync();
			}
		}

        public async Task OnGetWithAlert(ulong guildId, string message)
        {
            await OnGet(guildId);
            ViewData["Message"] = message;
        }

        public async Task<IActionResult> OnPostSave(List<ShopItem> SavedItems)
        {
            return RedirectToPage("Index","WithAlert",new { guildId = 0, message = $"Saved changes to 0" });
        }
    }
}
