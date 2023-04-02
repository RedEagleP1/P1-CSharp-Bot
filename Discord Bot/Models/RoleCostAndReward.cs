using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
	public class RoleCostAndReward
	{
		public int Id { get; set; }
        public ulong RoleId { get; set; }
        public float Cost { get; set; }
		public int? CostCurrencyId { get; set; }
		public float Reward { get; set; }
		public int? RewardCurrencyId { get; set; }		
	}
}
