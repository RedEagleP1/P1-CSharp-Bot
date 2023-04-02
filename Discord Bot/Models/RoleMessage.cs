using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class RoleMessage
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public ulong RoleId { get; set; }
    }
}
