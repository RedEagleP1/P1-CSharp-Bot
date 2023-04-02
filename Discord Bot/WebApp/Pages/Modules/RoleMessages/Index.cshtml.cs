using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.HelperClasses;
using System.Collections;

namespace WebApp.Pages.Modules.RoleMessages
{
    [Authorize(Policy = "Allowed")]
    public class IndexModel : PageModel
    {
        public Guild Guild { get; set; }
        public List<Role> AvailableRoles { get; set; }
        public List<RoleMessage_HM> RoleMessage_HMs { get; set; }
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
            var allRoleMessages = _db.RoleMessages.AsNoTracking().AsEnumerable().Where(rm => allRoles.Any(r => r.Id == rm.RoleId)).ToList();
            var availableRoles = new List<Role>();
            var roleMessage_HMs = new List<RoleMessage_HM>();

            foreach (var role in allRoles)
            {
                bool hasMessage = false;
                foreach (var roleMessage in allRoleMessages)
                {
                    if (roleMessage.RoleId == role.Id)
                    {
                        var roleMessage_HM = new RoleMessage_HM() { MainInstance = roleMessage, RoleName = role.Name };
                        roleMessage_HMs.Add(roleMessage_HM);
                        hasMessage = true;
                        break;
                    }
                }

                if (hasMessage)
                {
                    continue;
                }

                availableRoles.Add(role);
            }

            AvailableRoles = availableRoles;
            RoleMessage_HMs = roleMessage_HMs;
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
            var messageToDelete = await _db.RoleMessages.FirstOrDefaultAsync(rm => rm.RoleId == roleId);
            _db.RoleMessages.Remove(messageToDelete);
            await _db.SaveChangesAsync();
            var role = await _db.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roleId);
            return RedirectToPage(new { guildId = role.GuildId });
        }
        public async Task<IActionResult> OnPostSave(ulong roleId, string message)
        {
            var roleMessage = await _db.RoleMessages.FirstOrDefaultAsync(rm => rm.RoleId == roleId);
            roleMessage.Message = message;
            await _db.SaveChangesAsync();
            var role = await _db.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roleId);
            return RedirectToPage(new { guildId = role.GuildId });
        }
        public async Task<IActionResult> OnPostAddRoleMessage(ulong roleId)
        {
            if(!_db.Roles.Any(r => r.Id == roleId))
            {
                return BadRequest();
            }

            if(_db.RoleMessages.Any(rm => rm.RoleId == roleId))
            {
                return BadRequest();
            }

            var roleMessage = new RoleMessage()
            {
                RoleId = roleId,
                Message = "You got a new role"
            };

            _db.RoleMessages.Add(roleMessage);
            await _db.SaveChangesAsync();
            return new OkResult();
        }  
    }
}
