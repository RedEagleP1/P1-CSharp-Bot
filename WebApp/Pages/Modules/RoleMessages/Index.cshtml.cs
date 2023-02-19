using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;

namespace WebApp.Pages.Modules.RoleMessages
{
    [Authorize(Policy = "Allowed")]
    public class IndexModel : PageModel
    {
        public Guild? Guild { get; set; }
        public IEnumerable<Role> AvailableRoles { get; set; }

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

            AvailableRoles = Guild.Roles.Where(role => role.Message == null);
        }
        public async Task<IActionResult> OnPostDeleteRoleInteraction(string guildId, string roleId)
        {
            if (!ulong.TryParse(guildId, out ulong guild_Id) ||
                !ulong.TryParse(roleId, out ulong role_Id))
            {
                return BadRequest();
            }

            var guild = await _db.Guilds.Include(g => g.Roles).FirstOrDefaultAsync(g => g.Id == guild_Id);
            if (guild == null)
            {
                return BadRequest();
            }

            var role = guild.Roles.FirstOrDefault(r => r.Id == role_Id);
            if (role == null)
            {
                return BadRequest();
            }

            role.Message = null;
            role.HasSurvey = false;
            await _db.SaveChangesAsync();
            return new OkResult();
        }
        public async Task<IActionResult> OnPostAddRoleInteraction(string guildId, string roleId)
        {
            if (!ulong.TryParse(guildId, out ulong guild_Id) ||
                !ulong.TryParse(roleId, out ulong role_Id))
            {
                return BadRequest();
            }

            var guild = await _db.Guilds.Include(g => g.Roles).FirstOrDefaultAsync(g => g.Id == guild_Id);
            if (guild == null)
            {
                return BadRequest();
            }

            var role = guild.Roles.FirstOrDefault(r => r.Id == role_Id);
            if (role == null)
            {
                return BadRequest();
            }

            role.Message = $"You got role {role.Name}";
            role.HasSurvey = false;
            await _db.SaveChangesAsync();
            return new OkResult();
        }  
    }
}
