using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.HelperClasses;

namespace WebApp.Pages.Modules.RoleCostsAndRewards
{
    [Authorize(Policy = "Allowed")]
    public class IndexModel : PageModel
    {
        public Guild Guild { get; set; }
        public List<RoleCostAndReward_HM> RoleCostAndReward_HMs { get; set; }
        public bool IsEditMode { get; set; }
        public ulong EditRoleId { get; set; }
        public List<Currency> AllCurrencies { get; set; }

        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task OnGet(ulong guildId)
        {
            Guild = await _db.Guilds.AsNoTracking().FirstOrDefaultAsync(g => g.Id == guildId);
            var allRoles = _db.Roles.Where(r => r.GuildId == guildId).AsNoTracking().ToList();
            List<RoleCostAndReward_HM> roleCostAndReward_HMs = new();
            foreach (var role in allRoles)
            {
                var roleCostAndReward = await _db.RolesCostAndReward.AsNoTracking().FirstOrDefaultAsync(rcar => rcar.RoleId == role.Id);
                string costCurrencyName = "None";
                string rewardCurrencyName = "None";
                if (roleCostAndReward != null)
                {
                    if (roleCostAndReward.CostCurrencyId != null)
                    {
                        var costCurrency = await _db.Currencies.AsNoTracking().FirstOrDefaultAsync(rcar => rcar.Id == roleCostAndReward.CostCurrencyId);
                        costCurrencyName = costCurrency.Name;
                    }
                    if (roleCostAndReward.RewardCurrencyId != null)
                    {
                        var rewardCurrency = await _db.Currencies.AsNoTracking().FirstOrDefaultAsync(rcar => rcar.Id == roleCostAndReward.RewardCurrencyId);
                        rewardCurrencyName = rewardCurrency.Name;
                    }
                }
                else
                {
                    roleCostAndReward = new RoleCostAndReward()
                    {
                        Cost = 0,
                        Reward = 0,
                        RoleId = role.Id
                    };
                }

                var roleCostAndReward_HM = new RoleCostAndReward_HM()
                {
                    MainInstance = roleCostAndReward,
                    CostCurrencyName = costCurrencyName,
                    RewardCurrencyName = rewardCurrencyName,
                    RoleName = role.Name
                };

                roleCostAndReward_HMs.Add(roleCostAndReward_HM);
            }

            RoleCostAndReward_HMs = roleCostAndReward_HMs;
            AllCurrencies = _db.Currencies.AsNoTracking().ToList();           
        }

        public async Task OnGetEdit(ulong roleId)
        {
            var role = await _db.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roleId);
            await OnGet(role.GuildId);
            IsEditMode = true;
            EditRoleId = role.Id;
        }

        public async Task<IActionResult> OnPostSave(RoleCostAndReward_HM roleCostAndReward_HM)
        {
            var roleCostAndReward = roleCostAndReward_HM.MainInstance;
            var role = await _db.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roleCostAndReward.RoleId);
            if (roleCostAndReward.CostCurrencyId == null && roleCostAndReward.Cost != 0)
            {
                roleCostAndReward.Cost = 0;
            }
            if (roleCostAndReward.RewardCurrencyId == null && roleCostAndReward.Reward != 0)
            {
                roleCostAndReward.Reward = 0;
            }
            _db.RolesCostAndReward.Update(roleCostAndReward);
            await _db.SaveChangesAsync();
            return RedirectToPage("Index", new { guildId = role.GuildId });
        }
    }
}
