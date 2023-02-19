using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Pages.Home
{
    [Authorize(Policy = "Allowed")]
    public class GuildsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public IEnumerable<Guild> Guilds { get; set; }

        public GuildsModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task OnGet()
        {
            Guilds = _db.Guilds.Where(guild => guild.IsCurrentlyJoined);
        }
    }
}
