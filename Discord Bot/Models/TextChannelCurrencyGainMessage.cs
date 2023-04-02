using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
	public class TextChannelCurrencyGainMessage
	{
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public string ChannelName { get; set; }
        public bool IsEnabled { get; set; }
        public int? CurrencyId { get; set; }
        public int AmountGainedPerMessage { get; set; }
        public int DelayBetweenAllowedMessageInMinutes { get; set; }
    }
}
