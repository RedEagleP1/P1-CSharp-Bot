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
        public ulong emojiId { get; set; }
        public ulong CurrencyId { get; set; }
        public int Cost { get; set; }
        public ulong ItemEffectID { get; set; }
        public string ItemEffectVal { get; set; }
    }
}
