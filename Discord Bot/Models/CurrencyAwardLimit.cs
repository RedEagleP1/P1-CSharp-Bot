using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class CurrencyAwardLimit
    {
        public int Id { get; set; }
        public int CurrencyId { get; set; }
        public float AmountLeft { get; set; }
        public ulong AwarderId { get; set; }
    }
}
