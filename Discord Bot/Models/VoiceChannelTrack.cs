using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
	public class VoiceChannelTrack
	{
		public int Id { get; set; }
		public ulong UserId { get; set; }
		public ulong? ChannelId { get; set; }
		public bool IsMuteOrDeafen { get; set; }
		public DateTime LastRecorded { get; set; } 
	}
}
