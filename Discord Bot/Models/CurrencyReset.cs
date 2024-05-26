using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class CurrencyReset
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public int DaysBetween { get; set; }
        public bool Auto { get; set; }
        public int? CurrencyId { get; set; }
        public int? DaysLeft { get; set; }
    }
}
