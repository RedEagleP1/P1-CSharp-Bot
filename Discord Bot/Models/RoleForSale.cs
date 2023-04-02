using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class RoleForSale
    {
        public int Id { get; set; }
        public ulong RoleId { get; set; }
    }
}
