using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;

namespace WebApp.Pages.Modules.RoleCosts
{
    public class IndexModel : PageModel
    {
        public Guild Guild { get; set; }

        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task OnGet(ulong guildId)
        {
            Guild = await _db.Guilds.AsNoTracking()
                .Include(g => g.Roles)
                .FirstOrDefaultAsync(g => g.Id == guildId);
        }

        public async Task<IActionResult> OnPostChangeRoleCost(string guildId, string roleId, string roleCost)
        {
            if (!ulong.TryParse(guildId, out ulong guild_Id) ||
                !ulong.TryParse(roleId, out ulong role_Id) ||
                !float.TryParse(roleCost, out float role_Cost))
            {
                return BadRequest();
            }

            var guild = await _db.Guilds.Include(g => g.Roles).FirstOrDefaultAsync(g => g.Id == guild_Id);
            if(guild == null)
            {
                return BadRequest();
            }

            var role = guild.Roles.FirstOrDefault(r => r.Id == role_Id);
            if(role == null)
            {
                return BadRequest();
            }

            role.Cost = role_Cost;
            await _db.SaveChangesAsync();
            return new OkResult();
        }
    }
}
