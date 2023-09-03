using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.HelperClasses
{
    public class TextChannel
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public string ChannelName { get; set; }
    }
}
