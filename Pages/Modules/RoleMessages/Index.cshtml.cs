using DiscordBot.Data;
using DiscordBot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Pages.Modules.RoleMessages
{
    [Authorize(Policy = "Allowed")]
    public class IndexModel : PageModel
    {
        public Guild? Guild { get; set; }
        public Dictionary<ulong, string> AvailableRoles { get; set; }
        public bool IsEditMode { get; set; } = false;
        public ulong EditRoleID { get; set; } = 0;
        //public List<RoleMessage> RoleMessages { get; set; }
        private readonly ApplicationDbContext _db;
        private readonly Bot.DiscordBotService _bs;
        public IndexModel(ApplicationDbContext db, Bot.DiscordBotService bs)
        {
            _db = db;
            _bs = bs;
        }

        public async Task OnGet(ulong guildID)
        {
            Guild = _db.Guilds.AsNoTracking().Include(g => g.RoleMessages).FirstOrDefault(Guild => Guild.ID == guildID);
            var guildRoles = _bs.GetRoles(guildID);
            Dictionary<ulong, string> availableRoles = new Dictionary<ulong, string>();
            foreach (var gr in guildRoles)
            {
                var roleMessage = Guild.RoleMessages.Find(r => r.RoleID == gr.Key);
                if (roleMessage == null)
                {
                    availableRoles.Add(gr.Key, gr.Value);
                }
            }
            AvailableRoles = availableRoles;
        }

        public async Task<IActionResult> OnPostSelectRole(IFormCollection form)
        {
            var guildID = ulong.Parse(form["GuildID"]);
            var role = form["Role"].ToString().Split(",");
            
            var roleID = ulong.Parse(role[0]);
            var roleName = role[1];
            Guild guild = await _db.Guilds.FindAsync(guildID);
            guild.RoleMessages.Add(new RoleMessage()
            {
                RoleID = roleID,
                RoleName = roleName,
                Message = $"You got role {roleName}"
            });
            _db.Guilds.Update(guild);
            await _db.SaveChangesAsync();
            return RedirectToPage(new { guildID = guildID });
        }

        public async Task OnGetEditMode(ulong guildID, ulong roleID)
        {
            IsEditMode = true;
            EditRoleID = roleID;
            Guild = _db.Guilds.AsNoTracking().Include(g => g.RoleMessages).FirstOrDefault(Guild => Guild.ID == guildID);
            var guildRoles = _bs.GetRoles(guildID);
            Dictionary<ulong, string> availableRoles = new Dictionary<ulong, string>();
            foreach (var gr in guildRoles)
            {
                var roleMessage = Guild.RoleMessages.Find(r => r.RoleID == gr.Key);
                if (roleMessage == null)
                {
                    availableRoles.Add(gr.Key, gr.Value);
                }
            }
            AvailableRoles = availableRoles;
        }

        public async Task<IActionResult> OnPostSave(IFormCollection form)
        {
            var guildID = ulong.Parse(form["GuildID"]);
            var role = form["Role"].ToString().Split(",");
            var message = form["EditMessage"];
            var roleID = ulong.Parse(role[0]);
            var roleName = role[1];
            
            Guild guild = _db.Guilds.Include(g => g.RoleMessages).FirstOrDefault(Guild => Guild.ID == guildID);
            guild.RoleMessages.Find(rm => rm.RoleID == roleID).Message = message;
            _db.Guilds.Update(guild);
            await _db.SaveChangesAsync();
            return RedirectToPage(new { guildID = guildID });
        }

        public async Task<IActionResult> OnPostDelete(IFormCollection form)
        {
            var guildID = ulong.Parse(form["GuildID"]);
            var roleID = ulong.Parse(form["RoleID"]);
            Guild guild = _db.Guilds.Include(g => g.RoleMessages).FirstOrDefault(Guild => Guild.ID == guildID);
            for(int i=0;i<guild.RoleMessages.Count;i++)
            {
                if (guild.RoleMessages[i].RoleID == roleID)
                {
                    guild.RoleMessages.RemoveAt(i);
                    break;
                }
            }
            _db.Guilds.Update(guild);
            await _db.SaveChangesAsync();
            return RedirectToPage(new { guildID = guildID });
        }
    }
}
