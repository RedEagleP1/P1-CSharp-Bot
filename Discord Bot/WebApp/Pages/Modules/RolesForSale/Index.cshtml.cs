using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;

namespace WebApp.Pages.Modules.RolesForSale
{
    [Authorize(Policy = "Allowed")]
    public class IndexModel : PageModel
    {
        public Guild Guild { get; set; }
        public List<RoleForSaleModel> Roles { get; set; }
        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task OnGet(ulong guildId)
        {
            Guild = await _db.Guilds.AsNoTracking().FirstOrDefaultAsync(g => g.Id == guildId);
            var allRoles = _db.Roles.Where(r => r.GuildId == guildId).AsNoTracking().ToList();

            var rolesForSaleModel = new List<RoleForSaleModel>();
            foreach(var role in allRoles)
            {
                var roleForSaleModel = new RoleForSaleModel() { Role = role };
                var roleForSale = await _db.RolesForSale.FirstOrDefaultAsync(r => r.RoleId == role.Id);
                if(roleForSale != null)
                {
                    roleForSaleModel.IsForSale = true;
                }
                rolesForSaleModel.Add(roleForSaleModel);
            }

            Roles = rolesForSaleModel;
        }

        public async Task<IActionResult> OnPostChangeForSaleStatus(ulong roleId, bool isForSale)
        {
            if (!_db.Roles.Any(r => r.Id == roleId))
            {
                return BadRequest();
            }

            var roleForSale = await _db.RolesForSale.FirstOrDefaultAsync(r => r.RoleId == roleId);
            if(isForSale)
            {
                if(roleForSale == null)
                {
                    roleForSale = new() { RoleId = roleId };
                    _db.RolesForSale.Add(roleForSale);
                    await _db.SaveChangesAsync();
                }

                return new OkResult();
            }

            if(roleForSale != null)
            {
                _db.RolesForSale.Remove(roleForSale);
                await _db.SaveChangesAsync();
            }

            return new OkResult();
        }
    }

    public class RoleForSaleModel
    {
        public Role Role { get; set; }
        public bool IsForSale { get; set; }
    }
}
