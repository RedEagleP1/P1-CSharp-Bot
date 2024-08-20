using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
	public class ShopItem
	{
		public int? Id { get; set; }
		public ulong GuildId { get; set; }
        public string ItemName { get; set; }
        public string emojiId { get; set; }
        public int CurrencyId { get; set; }
        public int Cost { get; set; }
        public string Description { get; set; }
    }
}
