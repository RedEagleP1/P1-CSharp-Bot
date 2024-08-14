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
	}

	public class IdAuto
	{
		public int Id { get; set; }
		public int SelectedOption { get; set; }
		public int Type { get; set; }
		public string? Value { get; set; }
		public int? AutomationId { get; set; }
	}
}
