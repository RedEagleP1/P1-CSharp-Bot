using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
	public class ItemInventory
	{
		public ulong? id { get; set; }
		public ulong? itemId { get; set; }
		public ulong userId { get; set; }
		public ulong guildId { get; set; }
		public int amount { get; set; }
	}
}
