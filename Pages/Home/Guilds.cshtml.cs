using DiscordBot.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DiscordBot.Models;
using Microsoft.AspNetCore.Authorization;

namespace DiscordBot.Pages.Home
{
    [Authorize(Policy = "Allowed")]
    public class GuildsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly Bot.DiscordBotService _bs;
        public IEnumerable<Guild> Guilds { get; set; }

        public GuildsModel(ApplicationDbContext db, Bot.DiscordBotService bs)
        {
            _db = db;
            _bs = bs;
        }

        public async Task OnGet()
        {
            var guilds = _bs.GetGuilds();
            if(guilds == null)
            {
                Guilds = new List<Guild>();
                return;
            }
            var guildsInDB = _db.Guilds;
            bool isDirty = false;
            foreach(Guild guild in guilds)
            {
                Guild? found = guildsInDB.FirstOrDefault(g => g.ID == guild.ID);
                if(found == null)
                {
                    await _db.Guilds.AddAsync(guild);
                    isDirty = true;                    
                }
            }

            if(isDirty)
                await _db.SaveChangesAsync();

            Guilds = guilds;
        }
    }
}
