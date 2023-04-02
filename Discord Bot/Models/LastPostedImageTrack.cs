using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class LastPostedImageTrack
    {
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public ulong ChannelId { get; set; }        
        public DateTime LastRecordedPost { get; set; }
    }
}
