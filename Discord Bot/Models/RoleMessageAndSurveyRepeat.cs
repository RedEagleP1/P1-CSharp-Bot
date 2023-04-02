using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class RoleMessageAndSurveyRepeat
    {
        public int Id { get; set; }
        public ulong RoleId { get; set; }
        public TimeSpan RepeatTime { get; set; }
        public int RepeatAfterEvery_InDays { get; set; }
    }
}
