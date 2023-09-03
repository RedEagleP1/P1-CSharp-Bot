using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Threading.Channels;
using WebApp.Pages.Partials;
namespace WebApp.Pages.Modules.TextChannelMessageValidation
{
    [Authorize(Policy = "Allowed")]
    public class SelectedChannelModel : PageModel
    {
        public Guild Guild { get; set; }
        public Models.TextChannelMessageValidation TextChannelMessageValidation { get; set; }
        public string CurrencyName { get; set; }
        public string RoleOnSuccessName { get; set; }
        public string RoleOnFailureName { get; set; }
        public List<Currency> AllCurrencies { get; set; }
        public List<Role> AllRoles { get; set; }
        [TempData]
        public string Message { get; set; }
        private readonly ApplicationDbContext _db;
        public SelectedChannelModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task OnGet(ulong guildId, ulong channelId)
        {
            if (Message != null)
            {
                ViewData["Message"] = Message;
            }

            Guild = _db.Guilds.FirstOrDefault(g => g.Id == guildId);
            var textChannelMessageValidation = _db.TextChannelMessageValidation.FirstOrDefault(mv => mv.ChannelId == channelId);
            if(textChannelMessageValidation == null)
            {
                var textChannel = await DiscordREST.GetTextChannel(guildId, channelId);
                if(textChannel == null)
                {
                    return;
                }
                
                textChannelMessageValidation = new()
                {
                    GuildId = textChannel.GuildId,
                    ChannelId = textChannel.ChannelId,
                    ChannelName = textChannel.ChannelName
                };
            }

            CurrencyName = "None";
            RoleOnSuccessName = "None";
            RoleOnFailureName = "None";

            if(textChannelMessageValidation.CurrencyId != null)
            {
                var currency = await _db.Currencies.FirstOrDefaultAsync(c => c.Id == textChannelMessageValidation.CurrencyId);
                if(currency != null)
                {
                    CurrencyName = currency.Name;
                }                
            }
            if(textChannelMessageValidation.RoleToGiveSuccess != null)
            {
                var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == textChannelMessageValidation.RoleToGiveSuccess);
                if(role != null)
                {
                    RoleOnSuccessName = role.Name;
                }
            }
            if (textChannelMessageValidation.RoleToGiveFailure != null)
            {
                var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == textChannelMessageValidation.RoleToGiveFailure);
                if (role != null)
                {
                    RoleOnFailureName = role.Name;
                }
            }
            TextChannelMessageValidation = textChannelMessageValidation;
            AllCurrencies = _db.Currencies.ToList();
            AllRoles = _db.Roles.Where(r => r.GuildId == guildId).ToList();
        }
        public async Task<IActionResult> OnPostSave(Models.TextChannelMessageValidation TextChannelMessageValidation)
        {
            var textChannel = await DiscordREST.GetTextChannel(TextChannelMessageValidation.GuildId, TextChannelMessageValidation.ChannelId);
            if(textChannel == null)
            {
                return BadRequest();
            }

            TextChannelMessageValidation.ChannelName = textChannel.ChannelName;
            var textChannelFromDB = await _db.TextChannelMessageValidation.FirstOrDefaultAsync(mv => mv.Id == TextChannelMessageValidation.Id);
            if(textChannelFromDB == null)
            {
                _db.TextChannelMessageValidation.Add(TextChannelMessageValidation);
            }
            else
            {
                _db.Entry(textChannelFromDB).CurrentValues.SetValues(TextChannelMessageValidation);
            }

            await _db.SaveChangesAsync();
            TempData["Message"] = "Changes Saved";
            return RedirectToPage("SelectedChannel", new { guildId = textChannel.GuildId, channelId = textChannel.ChannelId });
        }
    }
}
