using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
	public class Automation
	{
		public ulong Id { get; set; }
		public ulong GuildId { get; set; }
		public ICollection<IdAuto> IdWhen { get; set; }
		public ICollection<InfoAuto> InfoWhen { get; set; }
		public ICollection<IdAuto> IdIf { get; set; }
		public ICollection<InfoAuto> InfoIf { get; set; }
		public ICollection<IdAuto> IdDo { get; set; }
		public ICollection<InfoAuto> InfoDo { get; set; }
	}

	public class IdAuto
	{
		public int Id { get; set; }
		public string Value { get; set; }
		public ulong AutomationId { get; set; }
	}

	public class InfoAuto
	{
		public int Id { get; set; }
		public string Value { get; set; }
		public ulong AutomationId { get; set; }
	}
}
