using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Models;

namespace WebApp.Pages.Modules.RoleMessages
{
    public class SelectModel : PageModel
    {
        public Guild Guild { get; set; }
        public Role Role { get; set; }

        private readonly ApplicationDbContext _db;
        public SelectModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task OnGet(ulong guildId, ulong roleId)
        {
            var guild = await _db.Guilds.AsNoTracking()
                .Include(g => g.Roles)
                .ThenInclude(r => r.RoleSurveys)
                
                .FirstOrDefaultAsync(g => g.Id == guildId);

            Guild = guild;
            Role = guild.Roles.FirstOrDefault(r => r.Id == roleId);
        }
        public async Task<IActionResult> OnPostChangeRoleMessage(string guildId, string roleId, string roleMessage)
        {
            if (!ulong.TryParse(guildId, out ulong guild_Id) ||
                !ulong.TryParse(roleId, out ulong role_Id) ||
                string.IsNullOrEmpty(roleMessage))
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

            role.Message = roleMessage;
            await _db.SaveChangesAsync();
            return new OkResult();
        }
        public async Task<IActionResult> OnPostNewRoleSurvey(string guildId, string roleId, string parentSurveyId)
        {
            if (!ulong.TryParse(guildId, out ulong guild_Id) ||
                !ulong.TryParse(roleId, out ulong role_Id) ||
                !ulong.TryParse(parentSurveyId, out ulong parentSurvey_Id))
            {
                return BadRequest();
            }

            var guild = await _db.Guilds.Include(g => g.Roles).AsNoTracking().FirstOrDefaultAsync(r => r.Id == role_Id);
            if (guild == null)
            {
                return BadRequest();
            }

            var role = await _db.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == role_Id);
            if (role == null)
            {
                return BadRequest();
            }

            if(parentSurvey_Id == 0)
            {
                var roleSurvey = new RoleSurvey()
                {
                    InitialMessage = "This is a new survey.",
                    HasConditionalTrigger = false,
                    SurveyOptions = new(),
                    AllowOptionsMultiSelect = false,
                    ChildSurveys = new()
                };

                return Partial("_RoleSurvey", new RoleSurveyPartialModel()
                {
                    CanHaveCondition = false,
                    RoleSurvey = roleSurvey,
                    AllRoles = guild.Roles
                });
            }

            return new OkResult();
        }
    }
}
