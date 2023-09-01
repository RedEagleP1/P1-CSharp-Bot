using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;
using WebApp.Pages.Partials;
using Models.HelperClasses;

namespace WebApp.Pages.Modules.TextChannelMessageValidation
{
    [Authorize(Policy = "Allowed")]
    public class IndexModel : PageModel
    {
        public Guild Guild { get; set; }
        public IEnumerable<TextChannelPartialModel> TextChannels { get; set; }

        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task OnGet(ulong guildId)
        {
            Guild = await _db.Guilds.FirstOrDefaultAsync(g => g.Id == guildId);
            var textChannelPartialModels = new List<TextChannelPartialModel>();
            var textChannels = await DiscordREST.GetTextChannelsAsync(guildId);
            foreach (var textChannel in textChannels)
            {
                var textChannelMessageValidation = await _db.TextChannelMessageValidation
                    .FirstOrDefaultAsync(mv => mv.GuildId == guildId && mv.ChannelId == textChannel.ChannelId);

                var textChannelPartialModel = new TextChannelPartialModel()
                {
                    GuildId = guildId,
                    ChannelId = textChannel.ChannelId,
                    ChannelName = textChannel.ChannelName,
                    IsEnabled = textChannelMessageValidation != null && textChannelMessageValidation.IsEnabled
                };

                textChannelPartialModels.Add(textChannelPartialModel);
            }

            TextChannels = textChannelPartialModels;
        }
    }
}
