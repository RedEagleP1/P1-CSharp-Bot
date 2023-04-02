using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.HelperClasses;
using WebApp.Pages.Partials;

namespace WebApp.Pages.Modules.RoleSurveys
{
    [Authorize(Policy = "Allowed")]
    public class IndexModel : PageModel
    {
        public Guild Guild { get; set; }
        public List<RoleListRowPartialModel> AllRoles { get; set; }

        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task OnGet(ulong guildId)
        {
            Guild = await _db.Guilds.AsNoTracking().FirstOrDefaultAsync(g => g.Id == guildId);
            var roles = _db.Roles.AsNoTracking().Where(r => r.GuildId == guildId).ToList();
            var allRoles = new List<RoleListRowPartialModel>();
            foreach(var role in roles)
            {
                var roleListRowPartial = new RoleListRowPartialModel()
                {
                    Role = role,
                    HasSurvey = _db.RolesSurvey.Any(rs => rs.RoleId == role.Id)
                };

                allRoles.Add(roleListRowPartial);
            }
            AllRoles = allRoles;
        }
    }
}
