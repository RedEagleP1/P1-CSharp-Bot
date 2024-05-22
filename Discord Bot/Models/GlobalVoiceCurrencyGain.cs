using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class GlobalVoiceCurrencyGain
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public bool IsEnabled { get; set; }
        public int? CurrencyId { get; set; }
        public float AmountGainedPerHourWhenMuteOrDeaf { get; set; }
        public float AmountGainedPerHourWhenSpeaking { get; set; }
    }
}
