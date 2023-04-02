using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class CurrencyOwned
    {
        public int Id { get; set; }
        public int CurrencyId { get; set; }
        public ulong OwnerId { get; set; }
        public float Amount { get; set; }
    }
}
