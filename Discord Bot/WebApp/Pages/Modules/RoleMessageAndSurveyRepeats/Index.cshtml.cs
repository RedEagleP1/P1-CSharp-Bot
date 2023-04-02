using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.HelperClasses;
using System.Collections;
namespace WebApp.Pages.Modules.RoleMessageAndSurveyRepeats
{
    [Authorize(Policy = "Allowed")]
    public class IndexModel : PageModel
    {
        public Guild Guild { get; set; }
        public List<Role> AvailableRoles { get; set; }
        public List<RoleMessageAndSurveyRepeat_HM> RoleMessageAndSurveyRepeat_HMs { get; set; }
        public bool IsEditMode { get; set; }
        public ulong EditRoleId { get; set; }

        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task OnGet(ulong guildId)
        {
            Guild = await _db.Guilds.AsNoTracking().FirstOrDefaultAsync(g => g.Id == guildId);

            var allRoles = _db.Roles.Where(r => r.GuildId == guildId).ToList();
            var allRepeats = _db.RoleMessageAndSurveyRepeats.AsNoTracking().AsEnumerable().Where(repeat => allRoles.Any(r => r.Id == repeat.RoleId)).ToList();
            var availableRoles = new List<Role>();
            var repeat_HMs = new List<RoleMessageAndSurveyRepeat_HM>();

            foreach (var role in allRoles)
            {
                bool hasRepeat = false;
                foreach (var repeat in allRepeats)
                {
                    if (repeat.RoleId == role.Id)
                    {
                        var repeat_HM = new RoleMessageAndSurveyRepeat_HM() { MainInstance = repeat, RoleName = role.Name };
                        repeat_HMs.Add(repeat_HM);
                        hasRepeat = true;
                        break;
                    }
                }

                if (hasRepeat)
                {
                    continue;
                }

                availableRoles.Add(role);
            }

            AvailableRoles = availableRoles;
            RoleMessageAndSurveyRepeat_HMs = repeat_HMs;
        }

        public async Task OnGetEdit(ulong roleId)
        {
            var role = await _db.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roleId);
            await OnGet(role.GuildId);
            IsEditMode = true;
            EditRoleId = roleId;
        }

        public async Task<IActionResult> OnGetDelete(ulong roleId)
        {
            var repeatToDelete = await _db.RoleMessageAndSurveyRepeats.FirstOrDefaultAsync(repeat => repeat.RoleId == roleId);
            _db.RoleMessageAndSurveyRepeats.Remove(repeatToDelete);
            await _db.SaveChangesAsync();
            var role = await _db.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roleId);
            return RedirectToPage(new { guildId = role.GuildId });
        }
        public async Task<IActionResult> OnPostSave(RoleMessageAndSurveyRepeat_HM roleMessageAndSurveyRepeat_HM)
        {
            if(roleMessageAndSurveyRepeat_HM.MainInstance.RepeatTime.TotalHours > 24 || roleMessageAndSurveyRepeat_HM.MainInstance.RepeatTime.TotalHours < 0)
            {
                roleMessageAndSurveyRepeat_HM.MainInstance.RepeatTime = TimeSpan.Zero;
            }
            _db.RoleMessageAndSurveyRepeats.Update(roleMessageAndSurveyRepeat_HM.MainInstance);
            await _db.SaveChangesAsync();
            var role = await _db.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roleMessageAndSurveyRepeat_HM.MainInstance.RoleId);
            return RedirectToPage(new { guildId = role.GuildId });
        }
        public async Task<IActionResult> OnPostAddRoleRepeat(ulong roleId)
        {
            if (!_db.Roles.Any(r => r.Id == roleId))
            {
                return BadRequest();
            }

            if (_db.RoleMessageAndSurveyRepeats.Any(rm => rm.RoleId == roleId))
            {
                return BadRequest();
            }

            var repeat = new RoleMessageAndSurveyRepeat()
            {
                RoleId = roleId,
                RepeatAfterEvery_InDays = 7,
                RepeatTime = TimeSpan.Zero
            };

            _db.RoleMessageAndSurveyRepeats.Add(repeat);
            await _db.SaveChangesAsync();
            return new OkResult();
        }
    }
}
