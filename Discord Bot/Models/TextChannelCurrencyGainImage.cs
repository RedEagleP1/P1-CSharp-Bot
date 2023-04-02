using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class TextChannelCurrencyGainImage
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public string ChannelName { get; set; }
        public bool IsEnabled { get; set; }
        public int? CurrencyId { get; set; }
        public int AmountGainedPerImagePost { get; set; }
        public int DelayBetweenAllowedImagePostInMinutes { get; set; }
    }
}
