using Models.HelperClasses;

namespace WebApp.Pages.Partials
{
    public class RoleCostAndRewardRowPartialModel
    {
        public RoleCostAndReward_HM RoleCostAndReward_HM { get; set; }
        public CurrencyDropdownPartialModel CurrencyDropdown { get; set; }
        public ulong GuildId { get; set; }
        public bool IsEditMode { get; set; }
    }
}
