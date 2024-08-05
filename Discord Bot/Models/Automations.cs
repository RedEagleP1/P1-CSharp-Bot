using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
	public class Automation
	{
		public int? Id { get; set; }
		public ulong GuildId { get; set; }
		public ICollection<IdAuto> When { get; set; }
		public ICollection<IdAuto> If { get; set; }
		public ICollection<IdAuto> Do { get; set; }
	}

	public class IdAuto
	{
		public int Id { get; set; }
		public string Value { get; set; }
		public int? AutomationId { get; set; }
	}
}
