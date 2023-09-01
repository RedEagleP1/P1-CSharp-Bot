using Models;

namespace WebApp.Pages.Partials
{
    public class TextChannelPartialModel
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public string ChannelName { get; set; }
        public bool IsEnabled { get; set; }
    }
}
